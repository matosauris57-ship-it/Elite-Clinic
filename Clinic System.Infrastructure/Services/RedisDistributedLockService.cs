namespace Clinic_System.Infrastructure.Services
{
    public class RedisDistributedLockService : IDistributedLockService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisDistributedLockService> _logger;
        // هنحفظ الـ Token لكل مفتاح عشان لما نيجي نفك القفل نضمن إننا بنفك قفلنا إحنا مش قفل حد تاني
        private readonly Dictionary<string, string> _lockTokens = new();

        public RedisDistributedLockService(IConnectionMultiplexer redis, ILogger<RedisDistributedLockService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<bool> AcquireLockAsync(string lockKey, TimeSpan expirationTime)
        {
            var db = _redis.GetDatabase();
            string token = Guid.NewGuid().ToString();

            bool isLocked = await db.LockTakeAsync(lockKey, token, expirationTime);

            if (isLocked)
            {
                _lockTokens[lockKey] = token;
                _logger.LogInformation("Successfully acquired lock for key: {LockKey}", lockKey);
            }
            else
            {
                _logger.LogWarning("Failed to acquire lock for key: {LockKey}. It might be held by another process.", lockKey);
            }

            return isLocked;
        }

        public async Task ReleaseLockAsync(string lockKey)
        {
            if (_lockTokens.TryGetValue(lockKey, out string token))
            {
                var db = _redis.GetDatabase();
                bool wasReleased = await db.LockReleaseAsync(lockKey, token);

                if (wasReleased)
                {
                    _lockTokens.Remove(lockKey);
                    _logger.LogInformation("Successfully released lock for key: {LockKey}", lockKey);
                }
                else
                {
                    _logger.LogWarning("Failed to release lock for key: {LockKey}. It may have already expired.", lockKey);
                }
            }
        }
    }
}
