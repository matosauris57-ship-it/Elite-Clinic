namespace Clinic_System.Infrastructure.Services
{
    /// <summary>No-op cache for local development without Redis.</summary>
    public class NullCacheService : ICacheService
    {
        public Task<T?> GetDataAsync<T>(string key) => Task.FromResult<T?>(default);

        public Task<bool> SetDataAsync<T>(string key, T value, TimeSpan expirationTime) => Task.FromResult(false);

        public Task<bool> RemoveDataAsync(string key) => Task.FromResult(false);

        public Task<bool> RemoveByPrefixAsync(params string[] prefixKeys) => Task.FromResult(false);

        public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> fetchData, TimeSpan cacheDuration) => fetchData();
    }
}
