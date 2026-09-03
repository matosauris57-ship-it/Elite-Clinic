namespace Clinic_System.Application.DTOs.AccessControl;

public class PermissionModuleDTO
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = [];
}

public class PermissionCatalogDTO
{
    public List<PermissionModuleDTO> Modules { get; set; } = [];
    public List<string> AllPermissions { get; set; } = [];
}

public class RoleListItemDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public int PermissionCount { get; set; }
}

public class RolePermissionsDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public List<string> Permissions { get; set; } = [];
}

public class ManagedUserDTO
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string? LinkedName { get; set; }
    public List<string> Roles { get; set; } = [];
    public bool IsLockedOut { get; set; }
    public bool EmailConfirmed { get; set; }
}

public class ManagedUserListDTO
{
    public List<ManagedUserDTO> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class CreateStaffUserResultDTO
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
