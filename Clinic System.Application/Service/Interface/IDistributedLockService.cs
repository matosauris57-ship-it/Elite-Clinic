namespace Clinic_System.Application.Service.Interface
{
    public interface IDistributedLockService
    {
        Task<bool> AcquireLockAsync(string lockKey, TimeSpan expirationTime);
        Task ReleaseLockAsync(string lockKey);
    }
}
