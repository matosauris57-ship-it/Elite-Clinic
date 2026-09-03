namespace Clinic_System.Application.Features.AccessControl.Queries.Models;

public class GetPermissionCatalogQuery : IRequest<Response<PermissionCatalogDTO>>;

public class GetRolesQuery : IRequest<Response<List<RoleListItemDTO>>>;

public class GetRolePermissionsQuery : IRequest<Response<RolePermissionsDTO>>
{
    public string RoleId { get; set; } = string.Empty;
}

public class GetUsersQuery : IRequest<Response<ManagedUserListDTO>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? UserType { get; set; }
    public string? RoleFilter { get; set; }
}
