using System.Text.Json;
using Clinic_System.Application.Common;
using Clinic_System.Core.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.API.Controllers
{
    [Route("api/campaign-booking")]
    [ApiController]
    public class CampaignBookingController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly AppDbContext _db;
        private readonly IAppointmentService _appointments;
        private readonly ITimeLimitedDataProtector _protector;

        public CampaignBookingController(
            AppDbContext db,
            IAppointmentService appointments,
            IDataProtectionProvider dataProtectionProvider)
        {
            _db = db;
            _appointments = appointments;
            _protector = dataProtectionProvider
                .CreateProtector("EliteClinic.CampaignBooking.v1")
                .ToTimeLimitedDataProtector();
        }

        [AllowAnonymous]
        [HttpGet("context")]
        public async Task<IActionResult> GetContext([FromQuery] string token, CancellationToken cancellationToken)
        {
            var payload = TryReadPayload(token);
            if (payload == null)
                return BadRequest(new { succeeded = false, message = "El enlace no es válido o expiró." });

            var recipient = await FindRecipientAsync(payload, cancellationToken);
            if (recipient == null)
                return NotFound(new { succeeded = false, message = "El enlace de la campaña ya no está disponible." });

            var doctors = await _db.Doctors
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.FullName)
                .Select(d => new
                {
                    d.Id,
                    d.FullName,
                    d.Specialization
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                succeeded = true,
                data = new
                {
                    patientName = recipient.PatientName,
                    campaignName = recipient.EmailCampaign.Name,
                    doctors,
                    flexibleSchedule = true,
                    note = "Puede elegir cualquier fecha y hora. Si el médico ya tiene una cita en ese momento, se le pedirá otro horario."
                }
            });
        }

        [AllowAnonymous]
        [HttpGet("occupied")]
        public async Task<IActionResult> GetOccupied(
            [FromQuery] string token,
            [FromQuery] int doctorId,
            [FromQuery] DateTime date,
            CancellationToken cancellationToken)
        {
            var payload = TryReadPayload(token);
            if (payload == null)
                return BadRequest(new { succeeded = false, message = "El enlace no es válido o expiró." });

            if (await FindRecipientAsync(payload, cancellationToken) == null)
                return NotFound(new { succeeded = false, message = "El enlace de la campaña ya no está disponible." });

            if (doctorId <= 0)
                return BadRequest(new { succeeded = false, message = "Seleccione un médico." });

            var booked = await _appointments.GetBookedAppointmentsAsync(doctorId, date.Date, cancellationToken);
            var times = booked
                .Select(a => a.AppointmentDate.TimeOfDay)
                .Distinct()
                .OrderBy(t => t)
                .Select(t => t.ToString(@"hh\:mm"))
                .ToList();

            return Ok(new { succeeded = true, data = times });
        }

        [AllowAnonymous]
        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] CampaignBookRequest request, CancellationToken cancellationToken)
        {
            var payload = TryReadPayload(request.Token);
            if (payload == null)
                return BadRequest(new { succeeded = false, message = "El enlace no es válido o expiró." });

            var recipient = await FindRecipientAsync(payload, cancellationToken);
            if (recipient == null)
                return NotFound(new { succeeded = false, message = "El enlace de la campaña ya no está disponible." });

            if (request.DoctorId <= 0)
                return BadRequest(new { succeeded = false, message = "Seleccione un médico." });

            if (request.AppointmentDate.Date < DateTime.Today
                || request.AppointmentDate.Date.Add(request.AppointmentTime) <= DateTime.Now)
            {
                return BadRequest(new { succeeded = false, message = "La fecha y hora deben ser posteriores a ahora." });
            }

            try
            {
                var appointment = await _appointments.BookAppointmentAsync(
                    recipient.PatientId,
                    request.DoctorId,
                    request.AppointmentDate.Date,
                    request.AppointmentTime,
                    cancellationToken,
                    allowFlexibleSchedule: true);

                var doctor = await _db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

                return Ok(new
                {
                    succeeded = true,
                    message = "Cita agendada. La clínica confirmará su asistencia.",
                    data = new
                    {
                        appointmentId = appointment.Id,
                        patientName = recipient.PatientName,
                        doctorName = doctor?.FullName,
                        appointmentDate = appointment.AppointmentDate,
                        status = appointment.Status.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { succeeded = false, message = ex.Message });
            }
        }

        private async Task<EmailCampaignRecipient?> FindRecipientAsync(
            CampaignBookingPayload payload,
            CancellationToken cancellationToken) =>
            await _db.EmailCampaignRecipients
                .Include(r => r.EmailCampaign)
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r =>
                    r.Id == payload.RecipientId
                    && r.EmailCampaignId == payload.CampaignId
                    && r.PatientId == payload.PatientId,
                    cancellationToken);

        private CampaignBookingPayload? TryReadPayload(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var json = _protector.Unprotect(token);
                return JsonSerializer.Deserialize<CampaignBookingPayload>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private sealed record CampaignBookingPayload(int CampaignId, int RecipientId, int PatientId);
    }

    public class CampaignBookRequest
    {
        public string Token { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
    }
}
