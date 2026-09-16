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

    public bool RestrictsToOwnDoctorData(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        if (user.IsInRole(AdminPermissionCatalog.SystemRoles.Admin))
            return false;

        if (!user.IsInRole(AdminPermissionCatalog.SystemRoles.Doctor))
            return false;

        return !HasPermission(user, AdminPermissionCatalog.ViewAllClinicData);
    }

    public int? GetDoctorId(ClaimsPrincipal? user)
    {
        var value = user?.FindFirst("DoctorId")?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }

    public bool CanAccessPanel(ClaimsPrincipal? user) =>
        user?.Identity?.IsAuthenticated == true &&
        AdminPermissionCatalog.CanAccessAdminPanel(
            user.FindAll(ClaimTypes.Role).Select(c => c.Value),
            user.FindAll(ClaimType).Select(c => c.Value));

    /// <summary>
    /// First admin page the user can open. Login always sent people to "/" (dashboard.view),
    /// which cookie-auth then treated as forbidden and redirected to a missing /Account/AccessDenied.
    /// </summary>
    public static readonly (string Permission, string Path)[] LandingPages =
    [
        ("dashboard.view", "/"),
        ("agendar-cita.view", "/agendar-cita"),
        ("agenda.view", "/agenda"),
        ("sala-espera.view", "/sala-espera"),
        ("pacientes.view", "/pacientes"),
        ("enfermedades.view", "/enfermedades"),
        ("medicos.view", "/medicos"),
        ("tratamientos.view", "/tratamientos"),
        ("historial.view", "/historial"),
        ("facturacion.view", "/facturacion"),
        ("campanas.view", "/campanas"),
        ("analitica.view", "/analitica"),
        ("inventario.view", "/inventario"),
        ("reportes.view", "/reportes"),
        ("recuperacion-contrasena.view", "/configuracion/recuperacion-contrasena")
    ];

    public static string ResolveLandingPath(IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        if (roles.Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
            return "/";

        var granted = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var (permission, path) in LandingPages)
        {
            if (granted.Contains(permission))
                return path;
        }

        return "/acceso-denegado";
    }

    public string ResolveLandingPath(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return "/login";

        return ResolveLandingPath(
            user.FindAll(ClaimTypes.Role).Select(c => c.Value),
            user.FindAll(ClaimType).Select(c => c.Value));
    }

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
