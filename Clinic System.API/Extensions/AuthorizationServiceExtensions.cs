using Clinic_System.Infrastructure.Authorization;
using Clinic_System.Core.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Clinic_System.API.Extensions;

public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AdminPanelAccessHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminPermissionCatalog.AdminPanelPolicy, policy =>
                policy.Requirements.Add(new AdminPanelAccessRequirement()));

            foreach (var permission in AdminPermissionCatalog.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }

            RegisterCompositePolicy(options, "tratamientos.view+doctor",
                AdminPermissionCatalog.Build("tratamientos", AdminPermissionCatalog.Actions.View),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "tratamientos.create+doctor",
                AdminPermissionCatalog.Build("tratamientos", AdminPermissionCatalog.Actions.Create),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "tratamientos.edit+doctor",
                AdminPermissionCatalog.Build("tratamientos", AdminPermissionCatalog.Actions.Edit),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "odontograma.view+doctor+patient",
                AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.View),
                AdminPermissionCatalog.SystemRoles.Doctor,
                AdminPermissionCatalog.SystemRoles.Patient);

            RegisterCompositePolicy(options, "odontograma.edit+doctor",
                AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "periodontograma.view+doctor+patient",
                AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.View),
                AdminPermissionCatalog.SystemRoles.Doctor,
                AdminPermissionCatalog.SystemRoles.Patient);

            RegisterCompositePolicy(options, "periodontograma.edit+doctor",
                AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.Edit),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "recetas.view+doctor+patient",
                AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.View),
                AdminPermissionCatalog.SystemRoles.Doctor,
                AdminPermissionCatalog.SystemRoles.Patient);

            RegisterCompositePolicy(options, "recetas.edit+doctor",
                AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.Edit),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "planes-tratamiento.view+doctor+patient",
                AdminPermissionCatalog.Build("planes-tratamiento", AdminPermissionCatalog.Actions.View),
                AdminPermissionCatalog.SystemRoles.Doctor,
                AdminPermissionCatalog.SystemRoles.Patient);

            RegisterCompositePolicy(options, "planes-tratamiento.create+doctor",
                AdminPermissionCatalog.Build("planes-tratamiento", AdminPermissionCatalog.Actions.Create),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "planes-tratamiento.edit+doctor",
                AdminPermissionCatalog.Build("planes-tratamiento", AdminPermissionCatalog.Actions.Edit),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "agendar-cita.create+patient",
                AdminPermissionCatalog.Build("agendar-cita", AdminPermissionCatalog.Actions.Create),
                AdminPermissionCatalog.SystemRoles.Patient);

            RegisterCompositePolicy(options, "agenda.edit+doctor",
                AdminPermissionCatalog.Build("agenda", AdminPermissionCatalog.Actions.Edit),
                AdminPermissionCatalog.SystemRoles.Doctor);

            RegisterCompositePolicy(options, "agenda.edit+patient",
                AdminPermissionCatalog.Build("agenda", AdminPermissionCatalog.Actions.Edit),
                AdminPermissionCatalog.SystemRoles.Patient);

            RegisterCompositePolicy(options, "agenda.delete+patient",
                AdminPermissionCatalog.Build("agenda", AdminPermissionCatalog.Actions.Delete),
                AdminPermissionCatalog.SystemRoles.Patient);
        });

        return services;
    }

    private static void RegisterCompositePolicy(AuthorizationOptions options, string policyName, string permission, params string[] roles)
    {
        options.AddPolicy(policyName, policy => policy.Requirements.Add(new PermissionRequirement(permission, roles)));
    }
}
