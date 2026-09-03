namespace Clinic_System.Application.Service.Interface;

public interface IRoleManagementService
{
    Task<IReadOnlyList<(string Id, string Name, bool IsSystem, int PermissionCount)>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, string? RoleId)> CreateRoleAsync(string name, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default);
}
