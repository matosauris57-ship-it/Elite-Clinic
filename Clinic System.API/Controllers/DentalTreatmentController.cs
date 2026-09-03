namespace Clinic_System.API.Controllers
{
[Route("api/dental/treatments")]
    [ApiController]
    [Authorize]
    public class DentalTreatmentController : AppControllerBase
    {
        public DentalTreatmentController(IMediator mediator) : base(mediator) { }
[HttpGet("patient/{patientId:int}")]
        [Authorize(Policy = "tratamientos.view+doctor")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var response = await mediator.Send(new GetDentalTreatmentsByPatientQuery { PatientId = patientId });
            return NewResult(response);
        }
[HttpGet("appointment/{appointmentId:int}")]
        [Authorize(Policy = "tratamientos.view+doctor")]
        public async Task<IActionResult> GetByAppointment(int appointmentId)
        {
            var response = await mediator.Send(new GetDentalTreatmentsByAppointmentQuery { AppointmentId = appointmentId });
            return NewResult(response);
        }
[HttpPost]
        [Authorize(Policy = "tratamientos.create+doctor")]
        public async Task<IActionResult> Create([FromBody] CreateDentalTreatmentCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpGet]
        [Authorize(Policy = "tratamientos.view+doctor")]
        public async Task<IActionResult> GetAdminList([FromQuery] GetDentalTreatmentsAdminListQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("{treatmentId:int}")]
        [Authorize(Policy = "tratamientos.view+doctor")]
        public async Task<IActionResult> GetById(int treatmentId)
        {
            var response = await mediator.Send(new GetDentalTreatmentByIdQuery { Id = treatmentId });
            return NewResult(response);
        }
[HttpPut("{treatmentId:int}")]
        [Authorize(Policy = "tratamientos.edit+doctor")]
        public async Task<IActionResult> Update(int treatmentId, [FromBody] UpdateDentalTreatmentCommand command)
        {
            command.Id = treatmentId;
            var response = await mediator.Send(command);
            return NewResult(response);
        }
        [HttpPut("{treatmentId:int}/start")]
        [Authorize(Policy = "tratamientos.edit+doctor")]
        public async Task<IActionResult> Start(int treatmentId)
        {
            return NewResult(await mediator.Send(new StartDentalTreatmentCommand { TreatmentId = treatmentId }));
        }

        [HttpPut("{treatmentId:int}/complete")]
        [Authorize(Policy = "tratamientos.edit+doctor")]
        public async Task<IActionResult> Complete(int treatmentId, [FromBody] CompleteDentalTreatmentCommand? command)
        {
            command ??= new CompleteDentalTreatmentCommand();
            command.TreatmentId = treatmentId;
            return NewResult(await mediator.Send(command));
        }
[HttpPut("{treatmentId:int}/cancel")]
        [Authorize(Policy = "tratamientos.edit+doctor")]
        public async Task<IActionResult> Cancel(int treatmentId, [FromBody] CancelDentalTreatmentCommand? command)
        {
            var response = await mediator.Send(new CancelDentalTreatmentCommand
            {
                TreatmentId = treatmentId,
                Reason = command?.Reason
            });
            return NewResult(response);
        }
[HttpDelete("{treatmentId:int}")]
        [Authorize(Policy = "tratamientos.delete")]
        public async Task<IActionResult> Delete(int treatmentId)
        {
            var response = await mediator.Send(new SoftDeleteDentalTreatmentCommand { TreatmentId = treatmentId });
            return NewResult(response);
        }
    }
}
