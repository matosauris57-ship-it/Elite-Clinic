namespace Clinic_System.API.Controllers
{
[Route("api/treatment-procedures")]
    [ApiController]
    [Authorize]
    public class TreatmentProcedureController : AppControllerBase
    {
        public TreatmentProcedureController(IMediator mediator) : base(mediator) { }
[HttpGet]
        public async Task<IActionResult> GetList([FromQuery] bool activeOnly = false, [FromQuery] int? doctorId = null)
        {
            var response = await mediator.Send(new GetTreatmentProcedureListQuery { ActiveOnly = activeOnly, DoctorId = doctorId });
            return NewResult(response);
        }
[HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] int? doctorId = null)
        {
            var response = await mediator.Send(new GetTreatmentProcedureByIdQuery { Id = id, DoctorId = doctorId });
            return NewResult(response);
        }
[HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTreatmentProcedureCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTreatmentProcedureCommand command)
        {
            command.Id = id;
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await mediator.Send(new SoftDeleteTreatmentProcedureCommand { Id = id });
            return NewResult(response);
        }
    }
}
