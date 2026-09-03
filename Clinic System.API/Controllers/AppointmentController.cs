namespace Clinic_System.API.Controllers
{
[Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentController : AppControllerBase
    {
        public AppointmentController(IMediator mediator) : base(mediator)
        {
        }
[HttpGet("stats")]
        public async Task<IActionResult> GetAppointmentsStats([FromQuery] GetAdminAppointmentsStatsQuery query)
        {
            var response = await mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("AvailableSlots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] GetAvailableSlotQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("doctor")]
        public async Task<IActionResult> GetDoctorAppointments([FromQuery] GetDoctorAppointmentsQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("patient")]
        public async Task<IActionResult> GetPatientAppointments([FromQuery] GetPatientAppointmentsQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("agenda")]
        public async Task<IActionResult> GetAgenda([FromQuery] GetAdminAgendaQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("statusforadmin")]
        public async Task<IActionResult> GetAppointmentsByStatusForAdmin([FromQuery] GetAppointmentsByStatusForAdminQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("statusfordoctor")]
        public async Task<IActionResult> GetAppointmentsByStatusForDoctor([FromQuery] GetAppointmentsByStatusForDoctorQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("pastforpatient")]
        public async Task<IActionResult> GetPastAppointmentsForPatient([FromQuery] GetPastAppointmentsForPatientQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpGet("pastfordoctor")]
        public async Task<IActionResult> GetPastAppointmentsForDoctor([FromQuery] GetPastAppointmentsForDoctorQuery query)
        {
            var response = await mediator.Send(query);
            return NewResult(response);
        }
[HttpPost("book")]
        [Authorize(Policy = "agendar-cita.create+patient")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("confirm")]
        public async Task<IActionResult> ConfirmAppointment([FromBody] ConfirmAppointmentCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("complete")]
        public async Task<IActionResult> CompleteAppointment([FromBody] CompleteAppointmentCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("reschedule")]
        public async Task<IActionResult> RescheduleAppointment([FromBody] RescheduleAppointmentCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("noshow")]
        public async Task<IActionResult> NoShowAppointment([FromBody] NoShowAppointmentCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPut("cancel")]
        public async Task<IActionResult> CancelledAppointment([FromBody] CancelAppointmentCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }
[HttpPost("call/{appointmentId}")]
        public async Task<IActionResult> CallPatient([FromRoute] int appointmentId)
        {
            var command = new CallPatientCommand { AppointmentId = appointmentId };
            var response = await mediator.Send(command);
            return NewResult(response);
        }
    }
}