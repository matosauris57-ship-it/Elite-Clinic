namespace Clinic_System.Application.Service.Interface;

public interface IUserManagementService
{
    Task<(IReadOnlyList<ManagedUserInfo> Items, int TotalCount)> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? userType,
        string? roleFilter,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, string? UserId)> CreateStaffUserAsync(
        string userName,
        string email,
        string password,
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> AssignUserRolesAsync(
        string userId,
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> SetUserLockoutAsync(
        string userId,
        bool lockoutEnabled,
        CancellationToken cancellationToken = default);
}

public sealed class ManagedUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = "Staff";
    public string? LinkedName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public bool IsLockedOut { get; set; }
    public bool EmailConfirmed { get; set; }
}
