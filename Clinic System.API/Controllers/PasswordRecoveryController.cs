using Clinic_System.Application.Features.Authentication.Queries.Models;

namespace Clinic_System.API.Controllers;

[Route("api/password-recovery")]
[ApiController]
[Authorize]
public class PasswordRecoveryController : AppControllerBase
{
    public PasswordRecoveryController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Policy = "recuperacion-contrasena.view")]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        return NewResult(await mediator.Send(new GetPasswordRecoveryRequestsQuery { Status = status }));
    }

    [HttpGet("pending-count")]
    [Authorize(Policy = "recuperacion-contrasena.view")]
    public async Task<IActionResult> PendingCount()
    {
        return NewResult(await mediator.Send(new GetPasswordRecoveryPendingCountQuery()));
    }

    [HttpPut("{id:int}/resolve")]
    [Authorize(Policy = "recuperacion-contrasena.edit")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolvePasswordRecoveryCommand? command)
    {
        command ??= new ResolvePasswordRecoveryCommand();
        command.Id = id;
        command.Dismiss = false;
        return NewResult(await mediator.Send(command));
    }

    [HttpPut("{id:int}/dismiss")]
    [Authorize(Policy = "recuperacion-contrasena.edit")]
    public async Task<IActionResult> Dismiss(int id, [FromBody] ResolvePasswordRecoveryCommand? command)
    {
        command ??= new ResolvePasswordRecoveryCommand();
        command.Id = id;
        command.Dismiss = true;
        return NewResult(await mediator.Send(command));
    }
}
