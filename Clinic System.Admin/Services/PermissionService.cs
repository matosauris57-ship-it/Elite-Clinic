using System.Security.Claims;
using Clinic_System.Core.Authorization;

namespace DentalCare.Admin.Services;

public class PermissionService
{
    public const string ClaimType = AdminPermissionCatalog.ClaimType;

    public bool HasPermission(ClaimsPrincipal? user, string permission)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        if (user.IsInRole(AdminPermissionCatalog.SystemRoles.Admin))
            return true;

        return user.HasClaim(ClaimType, permission);
    }

    public bool CanAccessPanel(ClaimsPrincipal? user) =>
        user?.Identity?.IsAuthenticated == true &&
        AdminPermissionCatalog.CanAccessAdminPanel(
            user.FindAll(ClaimTypes.Role).Select(c => c.Value),
            user.FindAll(ClaimType).Select(c => c.Value));

    public string? GetDisplayRole(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        if (user.IsInRole(AdminPermissionCatalog.SystemRoles.Admin))
            return "Director";

        var staffRole = user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .FirstOrDefault(r => !AdminPermissionCatalog.SystemRoles.IsSystemRole(r));

        return staffRole ?? "Staff";
    }
}
