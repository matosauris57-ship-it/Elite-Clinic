using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Clinic_System.Application.Common;
using Clinic_System.Application.Service.Interface;
using Clinic_System.Core.Entities;
using Clinic_System.Core.Enums;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.API.Controllers
{
    [Route("api/campaign-booking")]
    [ApiController]
    public class CampaignBookingController : ControllerBase
    {
        public const string OtherServiceKey = "other";

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly AppDbContext _db;
        private readonly IAppointmentService _appointments;
        private readonly IEmailService _email;
        private readonly IEmailSettingsProvider _emailSettings;
        private readonly ICampaignBookingNotifySettingsService _notifySettings;
        private readonly ITimeLimitedDataProtector _protector;
        private readonly ILogger<CampaignBookingController> _logger;

        public CampaignBookingController(
            AppDbContext db,
            IAppointmentService appointments,
            IEmailService email,
            IEmailSettingsProvider emailSettings,
            ICampaignBookingNotifySettingsService notifySettings,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<CampaignBookingController> logger)
        {
            _db = db;
            _appointments = appointments;
            _email = email;
            _emailSettings = emailSettings;
            _notifySettings = notifySettings;
            _protector = dataProtectionProvider
                .CreateProtector("EliteClinic.CampaignBooking.v1")
                .ToTimeLimitedDataProtector();
            _logger = logger;
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

            var existing = await LoadExistingSummaryAsync(recipient.Id, cancellationToken);
            if (existing != null)
            {
                return Ok(new
                {
                    succeeded = true,
                    data = new
                    {
                        patientName = recipient.PatientName,
                        campaignName = recipient.EmailCampaign.Name,
                        doctors = Array.Empty<object>(),
                        services = Array.Empty<object>(),
                        flexibleSchedule = true,
                        alreadySubmitted = true,
                        existingRequest = existing,
                        note = string.Equals(existing.Status, nameof(CampaignAppointmentRequestStatus.Scheduled), StringComparison.OrdinalIgnoreCase)
                            ? "Ya tiene una cita registrada a partir de este enlace."
                            : "Ya envió una solicitud con este enlace. La clínica la está revisando."
                    }
                });
            }

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

            var services = await _db.TreatmentProcedures
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.DurationMinutes
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
                    services,
                    flexibleSchedule = true,
                    alreadySubmitted = false,
                    existingRequest = (object?)null,
                    note = "Elija médico, servicio, fecha y hora. Su solicitud llegará a la clínica para confirmarla; no se agenda automáticamente."
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

            var recipient = await FindRecipientAsync(payload, cancellationToken);
            if (recipient == null)
                return NotFound(new { succeeded = false, message = "El enlace de la campaña ya no está disponible." });

            if (await HasExistingRequestAsync(recipient.Id, cancellationToken))
                return BadRequest(new { succeeded = false, message = "Ya envió una solicitud con este enlace." });

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
        [HttpPost("request")]
        public async Task<IActionResult> RequestAppointment(
            [FromBody] CampaignBookRequest request,
            CancellationToken cancellationToken)
        {
            var payload = TryReadPayload(request.Token);
            if (payload == null)
                return BadRequest(new { succeeded = false, message = "El enlace no es válido o expiró." });

            var recipient = await FindRecipientAsync(payload, cancellationToken);
            if (recipient == null)
                return NotFound(new { succeeded = false, message = "El enlace de la campaña ya no está disponible." });

            if (await HasExistingRequestAsync(recipient.Id, cancellationToken))
            {
                var existing = await LoadExistingSummaryAsync(recipient.Id, cancellationToken);
                return Conflict(new
                {
                    succeeded = false,
                    message = "Ya envió una solicitud con este enlace.",
                    data = existing
                });
            }

            if (request.DoctorId <= 0)
                return BadRequest(new { succeeded = false, message = "Seleccione un médico." });

            var doctorExists = await _db.Doctors.AsNoTracking()
                .AnyAsync(d => d.Id == request.DoctorId && !d.IsDeleted, cancellationToken);
            if (!doctorExists)
                return BadRequest(new { succeeded = false, message = "El médico seleccionado no está disponible." });

            var isOther = string.Equals(request.ServiceKey, OtherServiceKey, StringComparison.OrdinalIgnoreCase);

            int? treatmentId = null;
            string? otherText = null;
            string serviceLabel;

            if (isOther)
            {
                otherText = request.OtherServiceText?.Trim();
                if (string.IsNullOrWhiteSpace(otherText))
                    return BadRequest(new { succeeded = false, message = "Indique el servicio que desea." });
                if (otherText.Length > 200)
                    return BadRequest(new { succeeded = false, message = "El servicio no puede superar 200 caracteres." });
                serviceLabel = otherText;
            }
            else if (request.TreatmentProcedureId is null or <= 0)
            {
                return BadRequest(new { succeeded = false, message = "Seleccione un servicio." });
            }
            else
            {
                var procedure = await _db.TreatmentProcedures.AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.Id == request.TreatmentProcedureId
                        && !p.IsDeleted
                        && p.IsActive, cancellationToken);
                if (procedure == null)
                    return BadRequest(new { succeeded = false, message = "Seleccione un servicio válido." });
                treatmentId = procedure.Id;
                serviceLabel = procedure.Name;
            }

            var requestedAt = request.AppointmentDate.Date.Add(request.AppointmentTime);
            if (requestedAt <= DateTime.Now)
                return BadRequest(new { succeeded = false, message = "La fecha y hora deben ser posteriores a ahora." });

            var entity = new CampaignAppointmentRequest
            {
                EmailCampaignId = recipient.EmailCampaignId,
                EmailCampaignRecipientId = recipient.Id,
                PatientId = recipient.PatientId,
                RequestedDoctorId = request.DoctorId,
                RequestedAt = DateTime.Now,
                RequestedAppointmentAt = requestedAt,
                TreatmentProcedureId = treatmentId,
                OtherServiceText = otherText,
                Status = CampaignAppointmentRequestStatus.Pending
            };

            _db.CampaignAppointmentRequests.Add(entity);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                var existing = await LoadExistingSummaryAsync(recipient.Id, cancellationToken);
                return Conflict(new
                {
                    succeeded = false,
                    message = "Ya envió una solicitud con este enlace.",
                    data = existing
                });
            }

            var doctorName = await _db.Doctors.AsNoTracking()
                .Where(d => d.Id == request.DoctorId)
                .Select(d => d.FullName)
                .FirstOrDefaultAsync(cancellationToken);

            await NotifyStaffAsync(
                recipient.PatientName,
                recipient.EmailCampaign.Name,
                doctorName ?? "Médico",
                serviceLabel,
                requestedAt,
                entity.Id,
                cancellationToken);

            return Ok(new
            {
                succeeded = true,
                message = "Solicitud enviada. La clínica la revisará y le confirmará la cita.",
                data = new
                {
                    requestId = entity.Id,
                    appointmentId = (int?)null,
                    patientName = recipient.PatientName,
                    doctorName,
                    appointmentDate = requestedAt,
                    serviceName = serviceLabel,
                    status = entity.Status.ToString(),
                    alreadySubmitted = true
                }
            });
        }

        /// <summary>Compatibilidad: el endpoint anterior redirige al flujo de solicitud.</summary>
        [AllowAnonymous]
        [HttpPost("book")]
        public Task<IActionResult> Book([FromBody] CampaignBookRequest request, CancellationToken cancellationToken)
            => RequestAppointment(request, cancellationToken);

        [Authorize(Policy = "campanas.view")]
        [HttpGet("requests")]
        public async Task<IActionResult> ListRequests(
            [FromQuery] string? status,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, 200);
            var query = _db.CampaignAppointmentRequests
                .AsNoTracking()
                .Include(r => r.EmailCampaign)
                .Include(r => r.Patient)
                .Include(r => r.RequestedDoctor)
                .Include(r => r.ScheduledDoctor)
                .Include(r => r.TreatmentProcedure)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<CampaignAppointmentRequestStatus>(status, true, out var parsed))
            {
                query = query.Where(r => r.Status == parsed);
            }

            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                succeeded = true,
                data = items.Select(MapStaffItem).ToList()
            });
        }

        [Authorize(Policy = "campanas.edit")]
        [HttpPost("requests/{id:int}/schedule")]
        public async Task<IActionResult> Schedule(
            int id,
            [FromBody] CampaignScheduleRequest body,
            CancellationToken cancellationToken)
        {
            var entity = await _db.CampaignAppointmentRequests
                .Include(r => r.EmailCampaignRecipient)
                .Include(r => r.Patient)
                .Include(r => r.RequestedDoctor)
                .Include(r => r.TreatmentProcedure)
                .Include(r => r.EmailCampaign)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(new { succeeded = false, message = "Solicitud no encontrada." });

            if (entity.Status != CampaignAppointmentRequestStatus.Pending)
                return BadRequest(new { succeeded = false, message = "Esta solicitud ya fue resuelta." });

            var doctorId = body.DoctorId > 0 ? body.DoctorId : entity.RequestedDoctorId;
            var doctor = await _db.Doctors.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted, cancellationToken);
            if (doctor == null)
                return BadRequest(new { succeeded = false, message = "Seleccione un médico válido." });

            var appointmentDate = body.AppointmentDate.Date;
            var appointmentTime = body.AppointmentTime;
            var finalAt = appointmentDate.Add(appointmentTime);
            if (finalAt <= DateTime.Now)
                return BadRequest(new { succeeded = false, message = "La fecha y hora deben ser posteriores a ahora." });

            try
            {
                var appointment = await _appointments.BookAppointmentAsync(
                    entity.PatientId,
                    doctorId,
                    appointmentDate,
                    appointmentTime,
                    cancellationToken,
                    treatmentProcedureId: entity.TreatmentProcedureId,
                    allowFlexibleSchedule: true);

                entity.Status = CampaignAppointmentRequestStatus.Scheduled;
                entity.AppointmentId = appointment.Id;
                entity.ScheduledDoctorId = doctorId;
                entity.ScheduledAppointmentAt = appointment.AppointmentDate;
                entity.ResolvedAt = DateTime.Now;
                entity.ResolvedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                entity.ResolvedByName = User.Identity?.Name;
                entity.StaffNote = string.IsNullOrWhiteSpace(body.StaffNote) ? null : body.StaffNote.Trim();

                await _db.SaveChangesAsync(cancellationToken);

                var notified = await NotifyPatientScheduledAsync(entity, doctor.FullName, cancellationToken);
                if (notified)
                {
                    entity.PatientNotified = true;
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return Ok(new
                {
                    succeeded = true,
                    message = notified
                        ? "Cita agendada y correo de confirmación enviado al paciente."
                        : "Cita agendada. No se pudo enviar el correo al paciente (revise SMTP o el correo del paciente).",
                    data = MapStaffItem(await ReloadForMapAsync(entity.Id, cancellationToken) ?? entity)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { succeeded = false, message = ex.Message });
            }
        }

        [Authorize(Policy = "campanas.edit")]
        [HttpPost("requests/{id:int}/reject")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] CampaignRejectRequest? body,
            CancellationToken cancellationToken)
        {
            var entity = await _db.CampaignAppointmentRequests
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(new { succeeded = false, message = "Solicitud no encontrada." });

            if (entity.Status != CampaignAppointmentRequestStatus.Pending)
                return BadRequest(new { succeeded = false, message = "Esta solicitud ya fue resuelta." });

            entity.Status = CampaignAppointmentRequestStatus.Rejected;
            entity.ResolvedAt = DateTime.Now;
            entity.ResolvedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            entity.ResolvedByName = User.Identity?.Name;
            entity.StaffNote = string.IsNullOrWhiteSpace(body?.StaffNote) ? null : body!.StaffNote.Trim();
            await _db.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                succeeded = true,
                message = "Solicitud rechazada.",
                data = MapStaffItem(await ReloadForMapAsync(entity.Id, cancellationToken) ?? entity)
            });
        }

        private async Task NotifyStaffAsync(
            string patientName,
            string campaignName,
            string doctorName,
            string serviceLabel,
            DateTime requestedAt,
            int requestId,
            CancellationToken cancellationToken)
        {
            var settings = _notifySettings.Get();
            if (settings.Recipients.Count == 0)
            {
                _logger.LogWarning(
                    "Solicitud de campaña #{RequestId} creada sin destinatarios de aviso configurados.",
                    requestId);
                return;
            }

            if (!_emailSettings.IsConfigured())
            {
                _logger.LogWarning(
                    "Solicitud de campaña #{RequestId}: SMTP no configurado; no se avisó al personal.",
                    requestId);
                return;
            }

            var clinic = string.IsNullOrWhiteSpace(_emailSettings.Get().SenderName)
                ? "la clínica"
                : _emailSettings.Get().SenderName!;

            var publicUrl = _emailSettings.Get().PublicSiteUrl?.Trim().TrimEnd('/');
            var link = string.IsNullOrWhiteSpace(publicUrl)
                ? null
                : $"{publicUrl}/campanas/solicitudes";

            var subject = $"Nueva solicitud de cita (campaña) — {patientName}";
            var sb = new StringBuilder();
            sb.AppendLine($"Hay una nueva solicitud de cita desde una campaña en {clinic}.");
            sb.AppendLine();
            sb.AppendLine($"Paciente: {patientName}");
            sb.AppendLine($"Campaña: {campaignName}");
            sb.AppendLine($"Médico solicitado: {doctorName}");
            sb.AppendLine($"Servicio: {serviceLabel}");
            sb.AppendLine($"Fecha/hora solicitada: {requestedAt:dd/MM/yyyy hh:mm tt}");
            sb.AppendLine($"N.º de solicitud: #{requestId}");
            if (!string.IsNullOrWhiteSpace(link))
            {
                sb.AppendLine();
                sb.AppendLine($"Revisar en el panel: {link}");
            }

            var body = sb.ToString();
            foreach (var to in settings.Recipients)
            {
                try
                {
                    await _email.SendEmailAsync(to, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo avisar la solicitud de campaña a {Email}", to);
                }
            }
        }

        private async Task<bool> NotifyPatientScheduledAsync(
            CampaignAppointmentRequest entity,
            string doctorName,
            CancellationToken cancellationToken)
        {
            var email = entity.EmailCampaignRecipient?.Email
                ?? entity.Patient?.Email;
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!_emailSettings.IsConfigured())
                return false;

            var clinic = string.IsNullOrWhiteSpace(_emailSettings.Get().SenderName)
                ? "la clínica"
                : _emailSettings.Get().SenderName!;

            var finalAt = entity.ScheduledAppointmentAt ?? entity.RequestedAppointmentAt;
            var requestedAt = entity.RequestedAppointmentAt;
            var sameSlot = Math.Abs((finalAt - requestedAt).TotalMinutes) < 1
                && entity.ScheduledDoctorId == entity.RequestedDoctorId;

            var service = !string.IsNullOrWhiteSpace(entity.OtherServiceText)
                ? entity.OtherServiceText!
                : entity.TreatmentProcedure?.Name ?? "Consulta";

            var subject = sameSlot
                ? $"Su cita fue confirmada — {clinic}"
                : $"Su cita fue agendada — {clinic}";

            var sb = new StringBuilder();
            sb.AppendLine($"Hola {entity.EmailCampaignRecipient?.PatientName ?? entity.Patient?.FullName},");
            sb.AppendLine();
            if (sameSlot)
            {
                sb.AppendLine($"Confirmamos su cita en {clinic} con la fecha y hora que solicitó:");
            }
            else
            {
                sb.AppendLine($"Recibimos su solicitud de cita en {clinic}.");
                sb.AppendLine($"Usted había pedido: {requestedAt:dd/MM/yyyy} a las {requestedAt:hh:mm tt}.");
                sb.AppendLine("La clínica la agendó para la siguiente fecha:");
            }

            sb.AppendLine();
            sb.AppendLine($"Médico: {doctorName}");
            sb.AppendLine($"Servicio: {service}");
            sb.AppendLine($"Fecha: {finalAt:dd/MM/yyyy}");
            sb.AppendLine($"Hora: {finalAt:hh:mm tt}");
            sb.AppendLine();
            sb.AppendLine("Si necesita cambiarla, comuníquese con la clínica.");
            sb.AppendLine();
            sb.AppendLine($"Atentamente,{Environment.NewLine}{clinic}");

            try
            {
                await _email.SendEmailAsync(email, subject, sb.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar confirmación de cita de campaña al paciente {Email}", email);
                return false;
            }
        }

        private async Task<CampaignAppointmentRequest?> ReloadForMapAsync(int id, CancellationToken cancellationToken) =>
            await _db.CampaignAppointmentRequests
                .AsNoTracking()
                .Include(r => r.EmailCampaign)
                .Include(r => r.Patient)
                .Include(r => r.RequestedDoctor)
                .Include(r => r.ScheduledDoctor)
                .Include(r => r.TreatmentProcedure)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        private static object MapStaffItem(CampaignAppointmentRequest r)
        {
            var serviceName = !string.IsNullOrWhiteSpace(r.OtherServiceText)
                ? r.OtherServiceText!
                : r.TreatmentProcedure?.Name ?? "—";

            return new
            {
                id = r.Id,
                campaignName = r.EmailCampaign?.Name,
                patientId = r.PatientId,
                patientName = r.Patient?.FullName,
                patientPhone = r.Patient?.MobilePhone,
                patientEmail = r.Patient?.Email,
                requestedDoctorId = r.RequestedDoctorId,
                requestedDoctorName = r.RequestedDoctor?.FullName,
                requestedAppointmentAt = r.RequestedAppointmentAt,
                serviceName,
                isOtherService = !string.IsNullOrWhiteSpace(r.OtherServiceText),
                treatmentProcedureId = r.TreatmentProcedureId,
                status = r.Status.ToString(),
                appointmentId = r.AppointmentId,
                scheduledDoctorId = r.ScheduledDoctorId,
                scheduledDoctorName = r.ScheduledDoctor?.FullName,
                scheduledAppointmentAt = r.ScheduledAppointmentAt,
                requestedAt = r.RequestedAt,
                resolvedAt = r.ResolvedAt,
                resolvedByName = r.ResolvedByName,
                staffNote = r.StaffNote,
                patientNotified = r.PatientNotified
            };
        }

        private async Task<ExistingRequestSummary?> LoadExistingSummaryAsync(int recipientId, CancellationToken cancellationToken)
        {
            var existing = await _db.CampaignAppointmentRequests
                .AsNoTracking()
                .Include(r => r.RequestedDoctor)
                .Include(r => r.ScheduledDoctor)
                .Include(r => r.TreatmentProcedure)
                .FirstOrDefaultAsync(r => r.EmailCampaignRecipientId == recipientId, cancellationToken);

            if (existing == null)
                return null;

            var serviceName = !string.IsNullOrWhiteSpace(existing.OtherServiceText)
                ? existing.OtherServiceText!
                : existing.TreatmentProcedure?.Name ?? "—";

            var displayDoctor = existing.ScheduledDoctor?.FullName
                ?? existing.RequestedDoctor?.FullName;
            var displayAt = existing.ScheduledAppointmentAt ?? existing.RequestedAppointmentAt;

            return new ExistingRequestSummary(
                existing.Id,
                existing.AppointmentId,
                existing.Status.ToString(),
                displayDoctor,
                displayAt,
                existing.RequestedAppointmentAt,
                existing.ScheduledAppointmentAt,
                serviceName,
                true);
        }

        private Task<bool> HasExistingRequestAsync(int recipientId, CancellationToken cancellationToken) =>
            _db.CampaignAppointmentRequests.AnyAsync(r => r.EmailCampaignRecipientId == recipientId, cancellationToken);

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

        private sealed record ExistingRequestSummary(
            int RequestId,
            int? AppointmentId,
            string Status,
            string? DoctorName,
            DateTime AppointmentDate,
            DateTime? RequestedAppointmentAt,
            DateTime? ScheduledAppointmentAt,
            string? ServiceName,
            bool AlreadySubmitted);
    }

    public class CampaignBookRequest
    {
        public string Token { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public int? TreatmentProcedureId { get; set; }
        public string? ServiceKey { get; set; }
        public string? OtherServiceText { get; set; }
    }

    public class CampaignScheduleRequest
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string? StaffNote { get; set; }
    }

    public class CampaignRejectRequest
    {
        public string? StaffNote { get; set; }
    }
}
