namespace Clinic_System.API.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportsController : AppControllerBase
{
    public ReportsController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    [Authorize(Policy = "reportes.view")]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var response = await mediator.Send(new GetClinicReportsQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        });
        return NewResult(response);
    }
}
