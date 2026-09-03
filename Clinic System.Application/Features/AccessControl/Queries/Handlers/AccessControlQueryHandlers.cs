using Clinic_System.Core.Authorization;

namespace Clinic_System.Application.Features.AccessControl.Queries.Handlers;

public class GetPermissionCatalogQueryHandler : ResponseHandler, IRequestHandler<GetPermissionCatalogQuery, Response<PermissionCatalogDTO>>
{
    public Task<Response<PermissionCatalogDTO>> Handle(GetPermissionCatalogQuery request, CancellationToken cancellationToken)
    {
        var dto = new PermissionCatalogDTO
        {
            Modules = AdminPermissionCatalog.Modules
                .Select(m => new PermissionModuleDTO
                {
                    Key = m.Key,
                    DisplayName = m.DisplayName,
                    Actions = m.Actions.ToList()
                })
                .ToList(),
            AllPermissions = AdminPermissionCatalog.All.ToList()
        };

        return Task.FromResult(Success(dto));
    }
}

public class GetRolesQueryHandler : ResponseHandler, IRequestHandler<GetRolesQuery, Response<List<RoleListItemDTO>>>
{
    private readonly IRoleManagementService _roleManagementService;

    public GetRolesQueryHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Response<List<RoleListItemDTO>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleManagementService.GetRolesAsync(cancellationToken);
        var dto = roles.Select(r => new RoleListItemDTO
        {
            Id = r.Id,
            Name = r.Name,
            IsSystem = r.IsSystem,
            PermissionCount = r.PermissionCount
        }).ToList();

        return Success(dto);
    }
}

public class GetRolePermissionsQueryHandler : ResponseHandler, IRequestHandler<GetRolePermissionsQuery, Response<RolePermissionsDTO>>
{
    private readonly IRoleManagementService _roleManagementService;

    public GetRolePermissionsQueryHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Response<RolePermissionsDTO>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleManagementService.GetRolesAsync(cancellationToken);
        var role = roles.FirstOrDefault(r => r.Id == request.RoleId);
        if (role == default)
            return NotFound<RolePermissionsDTO>("Rol no encontrado.");

        var permissions = await _roleManagementService.GetRolePermissionsAsync(request.RoleId, cancellationToken);
        return Success(new RolePermissionsDTO
        {
            Id = role.Id,
            Name = role.Name,
            IsSystem = role.IsSystem,
            Permissions = permissions.ToList()
        });
    }
}

public class GetUsersQueryHandler : ResponseHandler, IRequestHandler<GetUsersQuery, Response<ManagedUserListDTO>>
{
    private readonly IUserManagementService _userManagementService;

    public GetUsersQueryHandler(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<Response<ManagedUserListDTO>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _userManagementService.GetUsersAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.UserType,
            request.RoleFilter,
            cancellationToken);

        var dto = new ManagedUserListDTO
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = total,
            Items = items.Select(u => new ManagedUserDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                UserType = u.UserType,
                LinkedName = u.LinkedName,
                Roles = u.Roles.ToList(),
                IsLockedOut = u.IsLockedOut,
                EmailConfirmed = u.EmailConfirmed
            }).ToList()
        };

        return Success(dto);
    }
}
