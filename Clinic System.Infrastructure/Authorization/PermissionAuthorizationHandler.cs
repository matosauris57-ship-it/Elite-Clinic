using Clinic_System.Core.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Clinic_System.Infrastructure.Authorization;

public static class AdminRoleAuthorization
{
    public static bool IsAdminUser(ClaimsPrincipal user)
    {
        if (user.IsInRole(AdminPermissionCatalog.SystemRoles.Admin))
            return true;

        return GetRoleValues(user).Any(r =>
            string.Equals(r, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<string> GetRoleValues(ClaimsPrincipal user) =>
        user.Claims
            .Where(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "roles" ||
                c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value);
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (AdminRoleAuthorization.IsAdminUser(context.User))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        foreach (var role in requirement.AllowedRoles)
        {
            if (context.User.IsInRole(role) ||
                AdminRoleAuthorization.GetRoleValues(context.User)
                    .Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        if (context.User.HasClaim(c =>
                c.Type == AdminPermissionCatalog.ClaimType &&
                string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public sealed class AdminPanelAccessRequirement : IAuthorizationRequirement;

public sealed class AdminPanelAccessHandler : AuthorizationHandler<AdminPanelAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminPanelAccessRequirement requirement)
    {
        var roles = AdminRoleAuthorization.GetRoleValues(context.User);
        var permissions = context.User.FindAll(AdminPermissionCatalog.ClaimType).Select(c => c.Value);

        if (AdminPermissionCatalog.CanAccessAdminPanel(roles, permissions))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
