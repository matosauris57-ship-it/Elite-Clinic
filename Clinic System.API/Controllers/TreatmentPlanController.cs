namespace Clinic_System.API.Controllers
{
[Route("api/dental/treatment-plans")]
    [ApiController]
    [Authorize]
    public class TreatmentPlanController : AppControllerBase
    {
        public TreatmentPlanController(IMediator mediator) : base(mediator) { }
[HttpGet("patient/{patientId:int}")]
        [Authorize(Policy = "planes-tratamiento.view+doctor+patient")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var response = await mediator.Send(new GetTreatmentPlansByPatientQuery { PatientId = patientId });
            return NewResult(response);
        }
[HttpPost]
        [Authorize(Policy = "planes-tratamiento.create+doctor")]
        public async Task<IActionResult> Create([FromBody] CreateTreatmentPlanCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("{planId:int}/approve")]
        [Authorize(Policy = "planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> Approve(int planId)
        {
            var response = await mediator.Send(new ApproveTreatmentPlanCommand { PlanId = planId });
            return NewResult(response);
        }

        [HttpPut("{planId:int}/reject")]
        [Authorize(Policy = "planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> Reject(int planId, [FromBody] RejectTreatmentPlanCommand? command)
        {
            command ??= new RejectTreatmentPlanCommand();
            command.PlanId = planId;
            return NewResult(await mediator.Send(command));
        }

        [HttpPut("{planId:int}/complete")]
        [Authorize(Policy = "planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> Complete(int planId)
        {
            return NewResult(await mediator.Send(new CompleteTreatmentPlanCommand { PlanId = planId }));
        }
    }
}
