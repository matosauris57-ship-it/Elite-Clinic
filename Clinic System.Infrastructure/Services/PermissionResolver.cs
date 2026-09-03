using Clinic_System.Core.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Services;

public class PermissionResolver : IPermissionResolver
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PermissionResolver(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<string>> ResolvePermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return [];

        var roles = await _userManager.GetRolesAsync(user);
        return await ResolvePermissionsForRolesAsync(roles, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ResolvePermissionsForRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        var roleList = roles.ToList();
        if (roleList.Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
            return AdminPermissionCatalog.All.ToList();

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in roleList)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                continue;

            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == AdminPermissionCatalog.ClaimType))
            {
                if (AdminPermissionCatalog.IsValid(claim.Value))
                    permissions.Add(claim.Value);
            }
        }

        return permissions.ToList();
    }
}
