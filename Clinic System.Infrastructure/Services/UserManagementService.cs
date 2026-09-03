using Clinic_System.Core.Authorization;
using Clinic_System.Core.Interfaces.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<(IReadOnlyList<ManagedUserInfo> Items, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? userType,
        string? roleFilter,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _userManager.Users.Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(term)));
        }

        var users = await query.OrderBy(u => u.Email).ToListAsync(cancellationToken);

        var doctors = await _unitOfWork.DoctorsRepository.GetAllAsync(cancellationToken);
        var patients = await _unitOfWork.PatientsRepository.GetAllAsync(cancellationToken);

        var doctorLinks = doctors
            .Where(d => !string.IsNullOrWhiteSpace(d.ApplicationUserId))
            .ToDictionary(d => d.ApplicationUserId, d => d.FullName);

        var patientLinks = patients
            .Where(p => !string.IsNullOrWhiteSpace(p.ApplicationUserId))
            .ToDictionary(p => p.ApplicationUserId, p => p.FullName);

        var mapped = new List<ManagedUserInfo>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var type = ResolveUserType(user.Id, roles, doctorLinks, patientLinks);
            var linkedName = doctorLinks.TryGetValue(user.Id, out var doctorName)
                ? doctorName
                : patientLinks.TryGetValue(user.Id, out var patientName) ? patientName : null;

            if (!string.IsNullOrWhiteSpace(userType) &&
                !string.Equals(type, userType, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrWhiteSpace(roleFilter) &&
                !roles.Any(r => string.Equals(r, roleFilter, StringComparison.OrdinalIgnoreCase)))
                continue;

            mapped.Add(new ManagedUserInfo
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                UserType = type,
                LinkedName = linkedName,
                Roles = roles,
                IsLockedOut = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                EmailConfirmed = user.EmailConfirmed
            });
        }

        var total = mapped.Count;
        var page = mapped
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (page, total);
    }

    public async Task<(bool Success, string? Error, string? UserId)> CreateStaffUserAsync(
        string userName,
        string email,
        string password,
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email?.Trim();
        var normalizedUserName = userName?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(normalizedUserName))
            return (false, "Nombre de usuario y email son obligatorios.", null);

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "La contraseña debe tener al menos 6 caracteres.", null);

        var rolesToAssign = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (rolesToAssign.Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
            return (false, "No se puede asignar el rol Admin al crear usuarios.", null);

        foreach (var roleName in rolesToAssign)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                return (false, $"El rol '{roleName}' no existe.", null);
        }

        var user = new ApplicationUser
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return (false, string.Join(", ", createResult.Errors.Select(e => e.Description)), null);

        foreach (var roleName in rolesToAssign)
            await _userManager.AddToRoleAsync(user, roleName);

        return (true, null, user.Id);
    }

    public async Task<(bool Success, string? Error)> AssignUserRolesAsync(
        string userId,
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
            return (false, "Usuario no encontrado.");

        var requested = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requested.Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
                return (false, "No se puede asignar el rol Admin desde esta pantalla.");
        }

        foreach (var roleName in requested)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                return (false, $"El rol '{roleName}' no existe.");
        }

        var existing = await _userManager.GetRolesAsync(user);
        var toRemove = existing.Where(r => !requested.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
        var toAdd = requested.Where(r => !existing.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

        if (toRemove.Any(r => string.Equals(r, AdminPermissionCatalog.SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)))
            return (false, "No se puede quitar el rol Admin.");

        if (toRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!removeResult.Succeeded)
                return (false, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
        }

        if (toAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, toAdd);
            if (!addResult.Succeeded)
                return (false, string.Join(", ", addResult.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SetUserLockoutAsync(
        string userId,
        bool lockoutEnabled,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.IsDeleted)
            return (false, "Usuario no encontrado.");

        if (await _userManager.IsInRoleAsync(user, AdminPermissionCatalog.SystemRoles.Admin))
            return (false, "No se puede desactivar la cuenta del administrador principal.");

        if (lockoutEnabled)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.SetLockoutEnabledAsync(user, false);
        }

        return (true, null);
    }

    private static string ResolveUserType(
        string userId,
        IReadOnlyList<string> roles,
        IReadOnlyDictionary<string, string> doctors,
        IReadOnlyDictionary<string, string> patients)
    {
        if (doctors.ContainsKey(userId))
            return "Doctor";

        if (patients.ContainsKey(userId))
            return "Patient";

        return "Staff";
    }
}
