namespace Clinic_System.API.Controllers
{
[Route("api/dental/history")]
    [ApiController]
    [Authorize]
    public class DentalHistoryController : AppControllerBase
    {
        public DentalHistoryController(IMediator mediator) : base(mediator) { }
[HttpGet("patient/{patientId:int}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var response = await mediator.Send(new GetDentalHistoryByPatientQuery { PatientId = patientId });
            return NewResult(response);
        }
[HttpPost]
        public async Task<IActionResult> Upsert([FromBody] UpsertDentalHistoryCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
    }
}
