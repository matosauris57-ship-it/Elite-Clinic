using Clinic_System.Application.DTOs.Dashboard;
using Clinic_System.Application.Features.Dashboard.Models;

namespace Clinic_System.API.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize]
public class DashboardController : AppControllerBase
{
    public DashboardController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("layout")]
    [Authorize(Policy = "dashboard.view")]
    public async Task<IActionResult> GetLayout(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetDashboardLayoutQuery(), cancellationToken);
        return NewResult(response);
    }

    [HttpPut("layout")]
    [Authorize(Policy = "dashboard.view")]
    public async Task<IActionResult> SaveLayout([FromBody] SaveDashboardLayoutCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);
        return NewResult(response);
    }

    [HttpPost("layout/restore")]
    [Authorize(Policy = "dashboard.view")]
    public async Task<IActionResult> RestoreLayout(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new RestoreDashboardLayoutCommand(), cancellationToken);
        return NewResult(response);
    }

    [HttpGet("clinic")]
    [Authorize(Policy = "configuracion.view")]
    public async Task<IActionResult> GetClinic(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetClinicDashboardConfigQuery(), cancellationToken);
        return NewResult(response);
    }

    [HttpPut("clinic")]
    [Authorize(Policy = "configuracion.view")]
    public async Task<IActionResult> SaveClinic([FromBody] SaveClinicDashboardConfigCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);
        return NewResult(response);
    }

    [HttpPost("clinic/restore")]
    [Authorize(Policy = "configuracion.view")]
    public async Task<IActionResult> RestoreClinic(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new RestoreClinicDashboardConfigCommand(), cancellationToken);
        return NewResult(response);
    }

    [HttpGet("patient-stats")]
    [Authorize(Policy = "pacientes.view")]
    public async Task<IActionResult> PatientStats(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetPatientDashboardStatsQuery(), cancellationToken);
        return NewResult(response);
    }

    [HttpGet("recent-activity")]
    [Authorize(Policy = "historial.view")]
    public async Task<IActionResult> RecentActivity([FromQuery] GetRecentClinicalActivityQuery query, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(query, cancellationToken);
        return NewResult(response);
    }

    [HttpGet("periodontal-incomplete")]
    [Authorize(Policy = "periodontograma.view")]
    public async Task<IActionResult> PeriodontalIncomplete(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetPeriodontalIncompleteStatsQuery(), cancellationToken);
        return NewResult(response);
    }
}
