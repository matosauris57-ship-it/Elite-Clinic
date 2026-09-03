namespace Clinic_System.API.Controllers
{
[Route("api/dental/odontogram")]
    [ApiController]
    [Authorize]
    public class OdontogramController : AppControllerBase
    {
        public OdontogramController(IMediator mediator) : base(mediator) { }
[HttpGet("patient/{patientId:int}")]
        [Authorize(Policy = "odontograma.view+doctor+patient")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var response = await mediator.Send(new GetToothRecordsByPatientQuery { PatientId = patientId });
            return NewResult(response);
        }
[HttpPost]
        [Authorize(Policy = "odontograma.edit+doctor")]
        public async Task<IActionResult> Upsert([FromBody] UpsertToothRecordCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPost("batch")]
        [Authorize(Policy = "odontograma.edit+doctor")]
        public async Task<IActionResult> BatchUpsert([FromBody] BatchUpsertOdontogramCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpGet("patient/{patientId:int}/chart")]
        [Authorize(Policy = "odontograma.view+doctor+patient")]
        public async Task<IActionResult> GetCurrentChart(
            int patientId,
            [FromQuery] string? dentition,
            [FromQuery] int? quadrant)
        {
            var response = await mediator.Send(new GetCurrentToothChartQuery
            {
                PatientId = patientId,
                Dentition = dentition,
                Quadrant = quadrant
            });
            return NewResult(response);
        }

        [HttpPost("entries")]
        [Authorize(Policy = "odontograma.edit+doctor")]
        public async Task<IActionResult> CreateEntry([FromBody] CreateToothChartEntryCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPost("entries/batch")]
        [Authorize(Policy = "odontograma.edit+doctor")]
        public async Task<IActionResult> CreateEntriesBatch([FromBody] CreateToothChartEntriesBatchCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpGet("patient/{patientId:int}/timeline")]
        [Authorize(Policy = "odontograma.view+doctor+patient")]
        public async Task<IActionResult> GetTimeline(int patientId, [FromQuery] int? toothNumber)
        {
            var response = await mediator.Send(new GetDentalTimelineQuery
            {
                PatientId = patientId,
                ToothNumber = toothNumber
            });
            return NewResult(response);
        }
    }
}
