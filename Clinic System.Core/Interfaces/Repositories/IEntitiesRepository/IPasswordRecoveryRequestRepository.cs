namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IPasswordRecoveryRequestRepository : IGenericRepository<PasswordRecoveryRequest>
{
    Task<PasswordRecoveryRequest?> FindRecentPendingAsync(string normalizedIdentifier, DateTime since, CancellationToken cancellationToken = default);
    Task<int> CountPendingAsync(CancellationToken cancellationToken = default);
    Task<List<PasswordRecoveryRequest>> ListAsync(PasswordRecoveryStatus? status, CancellationToken cancellationToken = default);
}
