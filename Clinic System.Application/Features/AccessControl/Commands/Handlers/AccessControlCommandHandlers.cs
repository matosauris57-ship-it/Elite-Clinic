namespace Clinic_System.Application.Features.AccessControl.Commands.Handlers;

public class CreateRoleCommandHandler : ResponseHandler, IRequestHandler<CreateRoleCommand, Response<RoleListItemDTO>>
{
    private readonly IRoleManagementService _roleManagementService;

    public CreateRoleCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Response<RoleListItemDTO>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var (success, error, roleId) = await _roleManagementService.CreateRoleAsync(request.Name, cancellationToken);
        if (!success || string.IsNullOrWhiteSpace(roleId))
            return BadRequest<RoleListItemDTO>(error ?? "No se pudo crear el rol.");

        return Created(new RoleListItemDTO
        {
            Id = roleId,
            Name = request.Name.Trim(),
            IsSystem = false,
            PermissionCount = 0
        }, string.Empty);
    }
}

public class UpdateRolePermissionsCommandHandler : ResponseHandler, IRequestHandler<UpdateRolePermissionsCommand, Response<string>>
{
    private readonly IRoleManagementService _roleManagementService;

    public UpdateRolePermissionsCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Response<string>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await _roleManagementService.UpdateRolePermissionsAsync(
            request.RoleId,
            request.Permissions,
            cancellationToken);

        return success ? Success("Permisos actualizados correctamente.") : BadRequest<string>(error ?? "No se pudieron actualizar los permisos.");
    }
}

public class DeleteRoleCommandHandler : ResponseHandler, IRequestHandler<DeleteRoleCommand, Response<string>>
{
    private readonly IRoleManagementService _roleManagementService;

    public DeleteRoleCommandHandler(IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    public async Task<Response<string>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await _roleManagementService.DeleteRoleAsync(request.RoleId, cancellationToken);
        return success ? Deleted<string>("Rol eliminado correctamente.") : BadRequest<string>(error ?? "No se pudo eliminar el rol.");
    }
}

public class CreateStaffUserCommandHandler : ResponseHandler, IRequestHandler<CreateStaffUserCommand, Response<CreateStaffUserResultDTO>>
{
    private readonly IUserManagementService _userManagementService;

    public CreateStaffUserCommandHandler(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<Response<CreateStaffUserResultDTO>> Handle(CreateStaffUserCommand request, CancellationToken cancellationToken)
    {
        var (success, error, userId) = await _userManagementService.CreateStaffUserAsync(
            request.UserName,
            request.Email,
            request.Password,
            request.RoleNames,
            cancellationToken);

        if (!success || string.IsNullOrWhiteSpace(userId))
            return BadRequest<CreateStaffUserResultDTO>(error ?? "No se pudo crear el usuario.");

        return Created(new CreateStaffUserResultDTO
        {
            UserId = userId,
            Email = request.Email.Trim()
        }, string.Empty);
    }
}

public class AssignUserRolesCommandHandler : ResponseHandler, IRequestHandler<AssignUserRolesCommand, Response<string>>
{
    private readonly IUserManagementService _userManagementService;

    public AssignUserRolesCommandHandler(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<Response<string>> Handle(AssignUserRolesCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await _userManagementService.AssignUserRolesAsync(
            request.UserId,
            request.RoleNames,
            cancellationToken);

        return success ? Success("Roles actualizados correctamente.") : BadRequest<string>(error ?? "No se pudieron actualizar los roles.");
    }
}

public class SetUserLockoutCommandHandler : ResponseHandler, IRequestHandler<SetUserLockoutCommand, Response<string>>
{
    private readonly IUserManagementService _userManagementService;

    public SetUserLockoutCommandHandler(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    public async Task<Response<string>> Handle(SetUserLockoutCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await _userManagementService.SetUserLockoutAsync(
            request.UserId,
            request.LockoutEnabled,
            cancellationToken);

        return success
            ? Success(request.LockoutEnabled ? "Usuario desactivado." : "Usuario activado.")
            : BadRequest<string>(error ?? "No se pudo actualizar el estado del usuario.");
    }
}
