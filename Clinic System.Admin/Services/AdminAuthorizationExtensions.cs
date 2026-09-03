using Clinic_System.Core.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace DentalCare.Admin.Services;

public static class AdminAuthorizationExtensions
{
    public static IServiceCollection AddAdminPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminPermissionCatalog.AdminPanelPolicy, policy =>
                policy.RequireAssertion(ctx =>
                    AdminPermissionCatalog.CanAccessAdminPanel(
                        ctx.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value),
                        ctx.User.FindAll(AdminPermissionCatalog.ClaimType).Select(c => c.Value))));

            foreach (var permission in AdminPermissionCatalog.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireAssertion(ctx =>
                        ctx.User.IsInRole(AdminPermissionCatalog.SystemRoles.Admin) ||
                        ctx.User.HasClaim(AdminPermissionCatalog.ClaimType, permission)));
            }
        });

        return services;
    }
}
