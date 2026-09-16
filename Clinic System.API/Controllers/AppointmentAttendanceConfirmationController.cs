using System.Text.Json;
using Clinic_System.Core.Entities;
using Microsoft.AspNetCore.DataProtection;

namespace Clinic_System.API.Controllers
{
    [Route("api/appointments/attendance-confirmation")]
    [ApiController]
    public class AppointmentAttendanceConfirmationController : ControllerBase
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(14);
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly AppDbContext _db;
        private readonly ITimeLimitedDataProtector _protector;

        public AppointmentAttendanceConfirmationController(
            AppDbContext db,
            IDataProtectionProvider dataProtectionProvider)
        {
            _db = db;
            _protector = dataProtectionProvider
                .CreateProtector("EliteClinic.AppointmentAttendanceConfirmation.v1")
                .ToTimeLimitedDataProtector();
        }

        [Authorize(Policy = "agenda.view")]
        [HttpPost("{appointmentId:int}/token")]
        public async Task<IActionResult> CreateToken(int appointmentId, CancellationToken cancellationToken)
        {
            var appointment = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

            if (appointment == null)
                return NotFound(new { succeeded = false, message = "Cita no encontrada." });

            var payload = new AttendanceTokenPayload(
                appointment.Id,
                appointment.PatientId,
                appointment.AppointmentDate);
            var token = _protector.Protect(JsonSerializer.Serialize(payload, JsonOptions), TokenLifetime);

            return Ok(new
            {
                succeeded = true,
                data = new
                {
                    token,
                    expiresAt = DateTimeOffset.UtcNow.Add(TokenLifetime),
                    patientName = appointment.Patient.FullName,
                    doctorName = appointment.Doctor.FullName,
                    appointmentDate = appointment.AppointmentDate,
                    status = appointment.Status.ToString()
                }
            });
        }

        [AllowAnonymous]
        [HttpGet("details")]
        public async Task<IActionResult> GetDetails([FromQuery] string token, CancellationToken cancellationToken)
        {
            var payload = TryReadPayload(token);
            if (payload == null)
                return BadRequest(new { succeeded = false, message = "El enlace no es válido o expiró." });

            var appointment = await FindAppointmentAsync(payload, cancellationToken);
            if (appointment == null)
                return NotFound(new { succeeded = false, message = "La cita ya no está disponible." });

            return Ok(new
            {
                succeeded = true,
                data = ToPublicDetails(appointment)
            });
        }

        [AllowAnonymous]
        [HttpPost("respond")]
        public async Task<IActionResult> Respond([FromBody] AttendanceConfirmationResponse request, CancellationToken cancellationToken)
        {
            var payload = TryReadPayload(request.Token);
            if (payload == null)
                return BadRequest(new { succeeded = false, message = "El enlace no es válido o expiró." });

            var appointment = await FindAppointmentAsync(payload, cancellationToken);
            if (appointment == null)
                return NotFound(new { succeeded = false, message = "La cita ya no está disponible." });

            if (appointment.AttendanceLinkRespondedAt != null || !CanRespondViaLink(appointment.Status))
            {
                return Ok(new
                {
                    succeeded = true,
                    message = SavedResponseMessage(appointment),
                    data = ToPublicDetails(appointment)
                });
            }

            var action = request.Action?.Trim().ToLowerInvariant();
            try
            {
                if (action == "confirm")
                    appointment.RespondViaAttendanceLink(true, request.Comment);
                else if (action == "decline")
                    appointment.RespondViaAttendanceLink(false, request.Comment);
                else
                    return BadRequest(new { succeeded = false, message = "Respuesta no válida." });

                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(new { succeeded = false, message = ex.Message });
            }

            return Ok(new
            {
                succeeded = true,
                message = action == "confirm"
                    ? "Asistencia confirmada. ¡Gracias!"
                    : "Hemos registrado que no podrá asistir.",
                data = ToPublicDetails(appointment)
            });
        }

        private static bool CanRespondViaLink(AppointmentStatus status) =>
            status is AppointmentStatus.Pending or AppointmentStatus.Rescheduled;

        private static string SavedResponseMessage(Appointment appointment)
        {
            if (appointment.AttendanceLinkAccepted == true)
                return "Su asistencia ya estaba confirmada.";
            if (appointment.AttendanceLinkAccepted == false)
                return "Ya habíamos registrado que no podrá asistir.";
            return "Esta cita ya tiene una respuesta registrada.";
        }

        private static object ToPublicDetails(Appointment appointment) => new
        {
            appointment.Id,
            patientName = appointment.Patient.FullName,
            doctorName = appointment.Doctor.FullName,
            appointmentDate = appointment.AppointmentDate,
            status = appointment.Status.ToString(),
            canRespond = appointment.AttendanceLinkRespondedAt == null && CanRespondViaLink(appointment.Status),
            alreadyResponded = appointment.AttendanceLinkRespondedAt != null,
            accepted = appointment.AttendanceLinkAccepted,
            comment = appointment.AttendanceLinkComment,
            respondedAt = appointment.AttendanceLinkRespondedAt
        };

        private async Task<Appointment?> FindAppointmentAsync(AttendanceTokenPayload payload, CancellationToken cancellationToken) =>
            await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a =>
                    a.Id == payload.AppointmentId &&
                    a.PatientId == payload.PatientId &&
                    a.AppointmentDate == payload.AppointmentDate,
                    cancellationToken);

        private AttendanceTokenPayload? TryReadPayload(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var json = _protector.Unprotect(token);
                return JsonSerializer.Deserialize<AttendanceTokenPayload>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private sealed record AttendanceTokenPayload(int AppointmentId, int PatientId, DateTime AppointmentDate);
    }

    public class AttendanceConfirmationResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }
}
