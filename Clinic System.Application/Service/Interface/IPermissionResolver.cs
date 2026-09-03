namespace Clinic_System.Application.Service.Interface;

public interface IPermissionResolver
{
    Task<IReadOnlyList<string>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ResolvePermissionsForRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default);
}
