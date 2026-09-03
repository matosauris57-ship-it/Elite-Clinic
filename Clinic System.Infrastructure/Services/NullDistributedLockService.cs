namespace Clinic_System.Infrastructure.Services
{
    /// <summary>No-op locks for local development without Redis.</summary>
    public class NullDistributedLockService : IDistributedLockService
    {
        public Task<bool> AcquireLockAsync(string lockKey, TimeSpan expirationTime) => Task.FromResult(true);

        public Task ReleaseLockAsync(string lockKey) => Task.CompletedTask;
    }
}
