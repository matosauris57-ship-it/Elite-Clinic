namespace Clinic_System.Infrastructure.Services
{
    internal class CacheItem<T>
    {
        public T? Data { get; set; }
        public DateTime LogicalExpiry { get; set; }
    }

    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IDistributedLockService _lockService;

        private static readonly JsonSerializerOptions _options =
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            };

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisCacheService> logger,
            IDistributedLockService lockService)
        {
            _db = connectionMultiplexer.GetDatabase();
            _logger = logger;
            _lockService = lockService;
        }

        public async Task<T?> GetDataAsync<T>(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);

                if (!value.HasValue)
                    return default;

                // الداتا بترجع كـ String (JSON)، بنحولها للـ Entity بتاعنا
                return JsonSerializer.Deserialize<T>(value.ToString(), _options);
            }
            catch (RedisConnectionException ex)
            {
                //  الـ Graceful Degradation: لو الـ Redis وقع، هنسجل الخطأ ونرجع null
                // الـ Handler لما يلاقيها null هيروح يجيبها من הـ SQL كأن مفيش كاش
                _logger.LogWarning(ex, "Redis is down! Failed to GET data for key: {Key}", key);
                return default;
            }
            catch (RedisTimeoutException ex)
            {
                _logger.LogWarning(ex, "Redis timeout! Failed to GET data for key: {Key}", key);
                return default;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "JSON deserialization failed for key: {Key}", key);
                return default;
            }
        }

        public async Task<bool> SetDataAsync<T>(string key, T value, TimeSpan expirationTime)
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value , _options);

                // بنستخدم StringSetAsync مع الـ Time To Live (TTL)
                return await _db.StringSetAsync(key, jsonValue, expirationTime);
            }
            catch (Exception ex) when (ex is RedisConnectionException || ex is RedisTimeoutException)
            {
                _logger.LogWarning(ex, "Redis is down! Failed to SET data for key: {Key}", key);
                return false; // فشل في الكاش، بس السيستم هيكمل عادي
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "JSON serialization failed for key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> RemoveDataAsync(string key)
        {
            try
            {
                // لو الداتا اتعدلت (مثلاً دكتور اتضاف)، لازم نمسح الكاش القديم
                bool isDeleted = await _db.KeyDeleteAsync(key);
                return isDeleted;
            }
            catch (Exception ex) when (ex is RedisConnectionException || ex is RedisTimeoutException)
            {
                _logger.LogWarning(ex, "Redis is down! Failed to REMOVE key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> RemoveByPrefixAsync(params string[] prefixKeys)
        {
            try
            {
                // 1. بنجيب الـ EndPoint اللي إحنا متصلين بيها (عشان ندور في الـ Server كله)
                var endpoint = _db.Multiplexer.GetEndPoints().First();
                var server = _db.Multiplexer.GetServer(endpoint);

                // هنعمل ليست نجمع فيها كل المفاتيح اللي لقيناها
                var allKeysToDelete = new List<RedisKey>();

                // نلف على كل الـ Prefixes اللي إنت بعتها (مثلاً: Profile_5, DoctorsList)
                foreach (var prefix in prefixKeys)
                {
                    var keys = server.Keys(pattern: $"{prefix}*").ToArray();
                    allKeysToDelete.AddRange(keys);
                }

                if (allKeysToDelete.Any())
                {
                    // 3. لو لقينا مفاتيح، نمسحها كلها بضربة واحدة
                    await _db.KeyDeleteAsync(allKeysToDelete.ToArray());

                    _logger.LogInformation("Cache invalidated! Deleted {Count} keys based on {PrefixCount} prefixes.",
                              allKeysToDelete.Count, prefixKeys.Length);
                    return true;
                }

                return false; // مفيش حاجة اتمسحت لأن الكاش كان فاضي أصلاً
            }
            catch (Exception ex) when (ex is RedisConnectionException || ex is RedisTimeoutException)
            {
                _logger.LogWarning(ex, "Redis is down! Failed to REMOVE keys by prefixs");
                return false;
            }
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> fetchData, TimeSpan cacheDuration)
        {
            try
            {
                var cachedJson = await _db.StringGetAsync(key);

                if (cachedJson.HasValue)
                {
                    var cacheItem = JsonSerializer.Deserialize<CacheItem<T>>(cachedJson.ToString(), _options);

                    if (cacheItem != null)
                    {
                        // ✅ الحالة الأولى: الكاش سليم وطازة
                        if (cacheItem.LogicalExpiry > DateTime.UtcNow)
                        {
                            return cacheItem.Data;
                        }

                        // ⚠️ الحالة التانية (الحل الأول): التجديد الاستباقي (Early Expiration)
                        // الوقت المنطقي خلص، بس الداتا لسه في الريديس!
                        string refreshLockKey = $"lock:refresh:{key}";

                        // بنحاول ناخد القفل لمدة 5 ثواني
                        bool gotRefreshLock = await _lockService.AcquireLockAsync(refreshLockKey, TimeSpan.FromSeconds(5));

                        if (gotRefreshLock)
                        {
                            try
                            {
                                // إحنا الريكويست "البطل" اللي هيجدد الداتا
                                _logger.LogInformation("Refreshing stale cache for {Key}", key);
                                var freshData = await fetchData();
                                await SaveToCacheInternalAsync(key, freshData, cacheDuration);
                                return freshData;
                            }
                            finally
                            {
                                await _lockService.ReleaseLockAsync(refreshLockKey);
                            }
                        }
                        else
                        {
                            // 🚀 ريكويست تاني سبقنا وبيجدد الكاش دلوقتي! 
                            // عشان الـ 10 آلاف يوزر ميستنوش، هنرجعله الداتا "القديمة" فوراً (Stale Data).
                            _logger.LogInformation("Cache is being refreshed by someone else. Returning stale data for {Key}", key);
                            return cacheItem.Data;
                        }
                    }
                }

                // 🛑 الحالة التالتة (الحل التاني): الكاش فاضي تماماً (Cache Stampede Protection)
                return await FetchWithStampedeLockAsync(key, fetchData, cacheDuration);
            }
            catch (Exception ex) when (ex is RedisConnectionException || ex is RedisTimeoutException)
            {
                _logger.LogWarning(ex, "Redis is down! Bypassing cache for {Key}", key);
                return await fetchData(); // لو الريديس وقع، بنكلم الـ DB فوراً
            }
        }

        //  دوال مساعدة (Helper Methods) لترتيب الكود
        private async Task<T?> FetchWithStampedeLockAsync<T>(string key, Func<Task<T>> fetchData, TimeSpan cacheDuration)
        {
            string fetchLockKey = $"lock:fetch:{key}";

            while (true) // بنلف لحد ما نجيب الداتا
            {
                // محاولة أخذ القفل لمدة 10 ثواني
                bool gotLock = await _lockService.AcquireLockAsync(fetchLockKey, TimeSpan.FromSeconds(10));

                if (gotLock)
                {
                    try
                    {
                        // 1. Double Check: يمكن ريكويست قبلي جاب الداتا وأنا كنت مستني!
                        var checkJson = await _db.StringGetAsync(key);
                        if (checkJson.HasValue)
                        {
                            var item = JsonSerializer.Deserialize<CacheItem<T>>(checkJson.ToString(), _options);
                            if (item != null) return item.Data;
                        }

                        // 2. مفيش مفر، هكلم الداتابيز
                        var newData = await fetchData();
                        await SaveToCacheInternalAsync(key, newData, cacheDuration);
                        return newData;
                    }
                    finally
                    {
                        await _lockService.ReleaseLockAsync(fetchLockKey);
                    }
                }
                else
                {
                    // ريكويست تاني بيكلم الداتابيز دلوقتي.. هنام 100 ملي ثانية وأصحى أجرب أشوف الكاش تاني
                    _logger.LogInformation("Waiting for another process to fetch data for {Key}", key);
                    await Task.Delay(100);

                    var retryJson = await _db.StringGetAsync(key);
                    if (retryJson.HasValue)
                    {
                        var item = JsonSerializer.Deserialize<CacheItem<T>>(retryJson.ToString(), _options);
                        if (item != null) return item.Data;
                    }
                }
            }
        }

        private async Task SaveToCacheInternalAsync<T>(string key, T data, TimeSpan logicalDuration)
        {
            var cacheItem = new CacheItem<T>
            {
                Data = data,
                LogicalExpiry = DateTime.UtcNow.Add(logicalDuration)
            };

            var jsonValue = JsonSerializer.Serialize(cacheItem, _options);

            // السر هنا: العمر الحقيقي في الريديس أطول من العمر المنطقي بـ 5 دقايق
            // ده اللي بيدينا "فترة سماح" نرجع فيها الداتا القديمة لحد ما الجديد يجي
            var physicalDuration = logicalDuration.Add(TimeSpan.FromMinutes(5));

            await _db.StringSetAsync(key, jsonValue, physicalDuration);
        }
    }
}