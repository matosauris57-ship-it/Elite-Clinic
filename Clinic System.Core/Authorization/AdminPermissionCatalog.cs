namespace Clinic_System.Core.Authorization;

public static class AdminPermissionCatalog
{
    public const string ClaimType = "permission";
    public const string AdminPanelPolicy = "admin.panel";

    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Patient = "Patient";

        public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
        {
            Admin, Doctor, Patient
        };

        public static bool IsSystemRole(string roleName) =>
            !string.IsNullOrWhiteSpace(roleName) && All.Contains(roleName);
    }

    public static class Actions
    {
        public const string View = "view";
        public const string Create = "create";
        public const string Edit = "edit";
        public const string Delete = "delete";
    }

    public sealed record ModuleDefinition(string Key, string DisplayName, IReadOnlyList<string> Actions);

    public static readonly IReadOnlyList<ModuleDefinition> Modules =
    [
        new("dashboard", "Dashboard", [Actions.View]),
        new("agendar-cita", "Agendar Cita", [Actions.View, Actions.Create]),
        new("agenda", "Agenda", [Actions.View, Actions.Edit, Actions.Delete]),
        new("sala-espera", "Sala de espera (TV)", [Actions.View]),
        new("pacientes", "Pacientes", [Actions.View, Actions.Create, Actions.Edit, Actions.Delete]),
        new("enfermedades", "Enfermedades", [Actions.View, Actions.Create, Actions.Edit, Actions.Delete]),
        new("medicos", "Médicos", [Actions.View, Actions.Create, Actions.Edit, Actions.Delete]),
        new("tratamientos", "Tratamientos", [Actions.View, Actions.Create, Actions.Edit, Actions.Delete]),
        new("odontograma", "Odontograma", [Actions.View, Actions.Edit]),
        new("periodontograma", "Periodontograma", [Actions.View, Actions.Edit]),
        new("planes-tratamiento", "Planes de tratamiento", [Actions.View, Actions.Create, Actions.Edit]),
        new("historial", "Historial Clínico", [Actions.View, Actions.Create, Actions.Edit]),
        new("recetas", "Recetas", [Actions.View, Actions.Create, Actions.Edit, Actions.Delete]),
        new("facturacion", "Facturación", [Actions.View, Actions.Create, Actions.Edit]),
        new("analitica", "Analítica", [Actions.View]),
        new("inventario", "Inventario", [Actions.View, Actions.Edit]),
        new("reportes", "Reportes", [Actions.View]),
        new("campanas", "Campañas de correo", [Actions.View, Actions.Create, Actions.Edit]),
        new("configuracion", "Configuración", [Actions.View]),
        new("usuarios", "Usuarios y roles", [Actions.View, Actions.Create, Actions.Edit, Actions.Delete])
    ];

    public static readonly IReadOnlyList<string> All = BuildAll();

    public static readonly IReadOnlyList<string> ViewPermissions = BuildViewPermissions();

    public static string Build(string moduleKey, string action) => $"{moduleKey}.{action}";

    public static bool IsValid(string permission) => All.Contains(permission, StringComparer.OrdinalIgnoreCase);

    public static bool IsViewPermission(string permission) =>
        permission.EndsWith($".{Actions.View}", StringComparison.OrdinalIgnoreCase);

    public static bool CanAccessAdminPanel(IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        if (roles.Any(r => string.Equals(r, SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
            return true;

        return permissions.Any(IsViewPermission);
    }

    private static IReadOnlyList<string> BuildAll()
    {
        var list = new List<string>();
        foreach (var module in Modules)
        {
            foreach (var action in module.Actions)
                list.Add(Build(module.Key, action));
        }

        return list;
    }

    private static IReadOnlyList<string> BuildViewPermissions()
    {
        return Modules
            .Where(m => m.Actions.Contains(Actions.View))
            .Select(m => Build(m.Key, Actions.View))
            .ToList();
    }
}
