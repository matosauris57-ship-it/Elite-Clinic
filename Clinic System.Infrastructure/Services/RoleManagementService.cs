using Clinic_System.Core.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleManagementService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<(string Id, string Name, bool IsSystem, int PermissionCount)>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync(cancellationToken);
        var result = new List<(string Id, string Name, bool IsSystem, int PermissionCount)>();

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var count = claims.Count(c => c.Type == AdminPermissionCatalog.ClaimType);
            result.Add((role.Id, role.Name ?? string.Empty, AdminPermissionCatalog.SystemRoles.IsSystemRole(role.Name ?? string.Empty), count));
        }

        return result;
    }

    public async Task<(bool Success, string? Error, string? RoleId)> CreateRoleAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return (false, "El nombre del rol es obligatorio.", null);

        if (AdminPermissionCatalog.SystemRoles.IsSystemRole(normalized))
            return (false, "No se puede crear un rol con nombre reservado del sistema.", null);

        if (await _roleManager.RoleExistsAsync(normalized))
            return (false, "Ya existe un rol con ese nombre.", null);

        var result = await _roleManager.CreateAsync(new IdentityRole(normalized));
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

        var role = await _roleManager.FindByNameAsync(normalized);
        return (true, null, role?.Id);
    }

    public async Task<(bool Success, string? Error)> UpdateRolePermissionsAsync(string roleId, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return (false, "Rol no encontrado.");

        if (string.Equals(role.Name, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase))
            return (false, "Los permisos del rol Admin no son editables.");

        var validPermissions = permissions
            .Where(AdminPermissionCatalog.IsValid)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var claim in existingClaims.Where(c => c.Type == AdminPermissionCatalog.ClaimType))
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        foreach (var permission in validPermissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim(AdminPermissionCatalog.ClaimType, permission));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return (false, "Rol no encontrado.");

        if (AdminPermissionCatalog.SystemRoles.IsSystemRole(role.Name ?? string.Empty))
            return (false, "No se pueden eliminar roles del sistema.");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    public async Task<IReadOnlyList<string>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return [];

        if (string.Equals(role.Name, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase))
            return AdminPermissionCatalog.All.ToList();

        var claims = await _roleManager.GetClaimsAsync(role);
        return claims
            .Where(c => c.Type == AdminPermissionCatalog.ClaimType && AdminPermissionCatalog.IsValid(c.Value))
            .Select(c => c.Value)
            .ToList();
    }
}
