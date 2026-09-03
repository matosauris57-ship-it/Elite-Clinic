namespace Clinic_System.Application.Features.AccessControl.Commands.Models;

public class CreateRoleCommand : IRequest<Response<RoleListItemDTO>>
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateRolePermissionsCommand : IRequest<Response<string>>
{
    public string RoleId { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
}

public class DeleteRoleCommand : IRequest<Response<string>>
{
    public string RoleId { get; set; } = string.Empty;
}

public class CreateStaffUserCommand : IRequest<Response<CreateStaffUserResultDTO>>
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> RoleNames { get; set; } = [];
}

public class AssignUserRolesCommand : IRequest<Response<string>>
{
    public string UserId { get; set; } = string.Empty;
    public List<string> RoleNames { get; set; } = [];
}

public class SetUserLockoutCommand : IRequest<Response<string>>
{
    public string UserId { get; set; } = string.Empty;
    public bool LockoutEnabled { get; set; }
}
