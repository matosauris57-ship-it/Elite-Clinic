namespace Clinic_System.API.Controllers;
[Route("api/access")]
[ApiController]
public class AccessControlController : AppControllerBase
{
    public AccessControlController(IMediator mediator) : base(mediator) { }
[HttpGet("permissions/catalog")]
    public async Task<IActionResult> GetPermissionCatalog()
    {
        var response = await mediator.Send(new GetPermissionCatalogQuery());
        return NewResult(response);
    }
[HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var response = await mediator.Send(new GetRolesQuery());
        return NewResult(response);
    }
[HttpGet("roles/{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        var response = await mediator.Send(new GetRolePermissionsQuery { RoleId = roleId });
        return NewResult(response);
    }
[HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        var response = await mediator.Send(command);
        return NewResult(response);
    }
[HttpPut("roles/{roleId}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsCommand command)
    {
        command.RoleId = roleId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }
[HttpDelete("roles/{roleId}")]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        var response = await mediator.Send(new DeleteRoleCommand { RoleId = roleId });
        return NewResult(response);
    }
[HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
    {
        var response = await mediator.Send(query);
        return NewResult(response);
    }
[HttpPost("users/staff")]
    public async Task<IActionResult> CreateStaffUser([FromBody] CreateStaffUserCommand command)
    {
        var response = await mediator.Send(command);
        return NewResult(response);
    }
[HttpPut("users/{userId}/roles")]
    public async Task<IActionResult> AssignUserRoles(string userId, [FromBody] AssignUserRolesCommand command)
    {
        command.UserId = userId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }
[HttpPut("users/{userId}/lockout")]
    public async Task<IActionResult> SetUserLockout(string userId, [FromBody] SetUserLockoutCommand command)
    {
        command.UserId = userId;
        var response = await mediator.Send(command);
        return NewResult(response);
    }
}
