namespace Clinic_System.Application.Service.Interface;

public interface IPasswordRecoveryService
{
    Task<PasswordRecoveryRequest> SubmitAsync(string identifier, string? comment, CancellationToken cancellationToken = default);
    Task<int> CountPendingAsync(CancellationToken cancellationToken = default);
    Task<List<PasswordRecoveryRequest>> ListAsync(PasswordRecoveryStatus? status, CancellationToken cancellationToken = default);
    Task<PasswordRecoveryRequest> ResolveAsync(int id, bool dismiss, string resolvedByUserId, string resolvedByName, string? note, CancellationToken cancellationToken = default);
}
