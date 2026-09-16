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

        [HttpGet("{planId:int}")]
        [Authorize(Policy = "planes-tratamiento.view+doctor+patient")]
        public async Task<IActionResult> GetById(int planId)
        {
            var response = await mediator.Send(new GetTreatmentPlanByIdQuery { PlanId = planId });
            return NewResult(response);
        }

        [HttpPost]
        [Authorize(Policy = "planes-tratamiento.create+doctor")]
        public async Task<IActionResult> Create([FromBody] CreateTreatmentPlanCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPut("{planId:int}/issue")]
        [Authorize(Policy = "planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> Issue(int planId)
        {
            return NewResult(await mediator.Send(new IssueTreatmentPlanCommand { PlanId = planId }));
        }

        [HttpPut("{planId:int}/approve")]
        [Authorize(Policy = "planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> Approve(int planId, [FromBody] ApproveTreatmentPlanCommand? command)
        {
            command ??= new ApproveTreatmentPlanCommand();
            command.PlanId = planId;
            return NewResult(await mediator.Send(command));
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

        [HttpPut("items/{itemId:int}/accept")]
        [Authorize(Policy = "planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> AcceptItem(int itemId)
        {
            return NewResult(await mediator.Send(new AcceptPlanItemCommand { ItemId = itemId }));
        }

        [HttpPut("items/{itemId:int}/reject")]
        [Authorize(Policy = "planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> RejectItem(int itemId)
        {
            return NewResult(await mediator.Send(new RejectPlanItemCommand { ItemId = itemId }));
        }

        [HttpPost("{planId:int}/invoice")]
        [Authorize(Policy = "facturacion.edit+planes-tratamiento.edit+doctor")]
        public async Task<IActionResult> Invoice(int planId, [FromBody] InvoiceTreatmentPlanCommand? command)
        {
            command ??= new InvoiceTreatmentPlanCommand();
            command.PlanId = planId;
            return NewResult(await mediator.Send(command));
        }
    }
}
