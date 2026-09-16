using Microsoft.AspNetCore.Authorization;

namespace Clinic_System.Infrastructure.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission, params string[] allowedRoles)
    {
        Permission = permission;
        AllowedRoles = allowedRoles ?? [];
    }

    public string Permission { get; }
    public IReadOnlyList<string> AllowedRoles { get; }
}

public sealed class AnyPermissionRequirement : IAuthorizationRequirement
{
    public AnyPermissionRequirement(params string[] permissions)
    {
        Permissions = permissions ?? [];
    }

    public IReadOnlyList<string> Permissions { get; }
}
