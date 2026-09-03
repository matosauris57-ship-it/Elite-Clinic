using Clinic_System.Core.Odontogram;
using Clinic_System.Core.Validation;
using ApiResponse = Clinic_System.Application.Common.Bases.Response<Clinic_System.Application.Common.ClinicOperatingHours>;
using EmailSettingsResponse = Clinic_System.Application.Common.Bases.Response<Clinic_System.Application.Common.ClinicEmailSettings>;
using NotificationSettingsResponse = Clinic_System.Application.Common.Bases.Response<Clinic_System.Application.Common.PatientNotificationSettings>;
using SendEmailResponse = Clinic_System.Application.Common.Bases.Response<string>;
using SymbolResponse = Clinic_System.Application.Common.Bases.Response<Clinic_System.Core.Odontogram.OdontogramSymbolConfigDocument>;

namespace Clinic_System.API.Controllers
{
    [Route("api/clinic")]
    [ApiController]
    [Authorize]
    public class ClinicController : AppControllerBase
    {
        private readonly IClinicOperatingHoursService _hours;
        private readonly IOdontogramSymbolConfigService _symbols;
        private readonly IEmailSettingsProvider _emailSettings;
        private readonly IEmailService _emailService;
        private readonly IPatientNotificationSettingsService _patientNotifications;

        public ClinicController(
            IMediator mediator,
            IClinicOperatingHoursService hours,
            IOdontogramSymbolConfigService symbols,
            IEmailSettingsProvider emailSettings,
            IEmailService emailService,
            IPatientNotificationSettingsService patientNotifications) : base(mediator)
        {
            _hours = hours;
            _symbols = symbols;
            _emailSettings = emailSettings;
            _emailService = emailService;
            _patientNotifications = patientNotifications;
        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetSchedule(CancellationToken cancellationToken)
        {
            var hours = await _hours.GetAsync(cancellationToken);
            return NewResult(OkSchedule(hours, "Horario de la clínica."));
        }

        [HttpPut("schedule")]
        [Authorize(Policy = "configuracion.view")]
        public async Task<IActionResult> SaveSchedule([FromBody] ClinicOperatingHours hours, CancellationToken cancellationToken)
        {
            if (hours.CloseTime <= hours.OpenTime)
                return NewResult(FailSchedule("La hora de cierre debe ser posterior a la de apertura."));

            if (hours.SlotDurationMinutes is < 5 or > 120)
                return NewResult(FailSchedule("La duración del turno debe estar entre 5 y 120 minutos."));

            var workingDays = (hours.WorkingDays ?? [])
                .Where(d => d is >= 0 and <= 6)
                .Distinct()
                .ToList();
            if (workingDays.Count == 0)
                return NewResult(FailSchedule("Seleccione al menos un día de trabajo."));

            hours.WorkingDays = workingDays;
            await _hours.SaveAsync(hours, cancellationToken);
            var saved = await _hours.GetAsync(cancellationToken);
            return NewResult(OkSchedule(saved, "Horario de la clínica guardado."));
        }

        [HttpGet("email-settings")]
        [Authorize(Policy = "configuracion.view")]
        public IActionResult GetEmailSettings()
        {
            return NewResult(OkEmail(ToPublic(_emailSettings.Get()), "Configuración de correo de la clínica."));
        }

        [HttpPut("email-settings")]
        [Authorize(Policy = "configuracion.view")]
        public async Task<IActionResult> SaveEmailSettings([FromBody] ClinicEmailSettings request, CancellationToken cancellationToken)
        {
            var error = ValidateEmailSettings(request, allowEmptyPassword: !string.IsNullOrWhiteSpace(_emailSettings.Get().Password));
            if (error != null)
                return NewResult(FailEmail(error));

            await _emailSettings.SaveAsync(new EmailSettings
            {
                Host = request.Host,
                Port = request.Port,
                FromEmail = request.FromEmail,
                SmtpUser = request.SmtpUser,
                Password = request.Password ?? string.Empty,
                SenderName = request.SenderName
            }, keepExistingPassword: string.IsNullOrWhiteSpace(request.Password), cancellationToken);

            return NewResult(OkEmail(ToPublic(_emailSettings.Get()), "Configuración de correo guardada."));
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendPatientEmail([FromBody] SendPatientEmailRequest request, CancellationToken cancellationToken)
        {
            if (!_emailSettings.IsConfigured())
                return NewResult(FailSend("Configure el SMTP en Configuración → Datos de la clínica."));

            if (!ContactEmail.TryValidate(request.To, out var to, out var emailError) || string.IsNullOrWhiteSpace(to))
                return NewResult(FailSend(emailError ?? "El paciente no tiene un correo válido."));

            if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
                return NewResult(FailSend("El asunto y el mensaje son obligatorios."));

            try
            {
                await _emailService.SendEmailAsync(to, request.Subject.Trim(), request.Body);
                return NewResult(new SendEmailResponse
                {
                    Succeeded = true,
                    StatusCode = HttpStatusCode.OK,
                    Data = to,
                    Message = "Correo enviado."
                });
            }
            catch (Exception)
            {
                return NewResult(FailSend("No se pudo enviar el correo. Revise el SMTP, usuario y contraseña."));
            }
        }

        [HttpGet("patient-notifications")]
        [Authorize(Policy = "configuracion.view")]
        public IActionResult GetPatientNotifications()
        {
            return NewResult(OkNotifications(_patientNotifications.Get(), "Avisos automáticos de la clínica."));
        }

        [HttpPut("patient-notifications")]
        [Authorize(Policy = "configuracion.view")]
        public async Task<IActionResult> SavePatientNotifications([FromBody] PatientNotificationSettings request, CancellationToken cancellationToken)
        {
            var error = ValidateNotifications(request);
            if (error != null)
                return NewResult(FailNotifications(error));

            await _patientNotifications.SaveAsync(request, cancellationToken);
            return NewResult(OkNotifications(_patientNotifications.Get(), "Avisos automáticos guardados."));
        }

        [HttpGet("odontogram/symbol-config")]
        public async Task<IActionResult> GetOdontogramSymbolConfig(CancellationToken cancellationToken)
        {
            var config = await _symbols.GetAsync(cancellationToken: cancellationToken);
            return NewResult(OkSymbols(config, "Simbología del odontograma."));
        }

        [HttpPut("odontogram/symbol-config")]
        [Authorize(Policy = "configuracion.view")]
        public async Task<IActionResult> SaveOdontogramSymbolConfig(
            [FromBody] OdontogramSymbolConfigDocument document,
            CancellationToken cancellationToken)
        {
            var error = OdontogramSymbolDefaults.Validate(OdontogramSymbolDefaults.Merge(document));
            if (error != null)
                return NewResult(FailSymbols(error));

            try
            {
                var saved = await _symbols.SaveAsync(document, CurrentUserName(), cancellationToken);
                return NewResult(OkSymbols(saved, "Simbología del odontograma guardada."));
            }
            catch (InvalidOperationException ex)
            {
                return NewResult(FailSymbols(ex.Message));
            }
        }

        [HttpPost("odontogram/symbol-config/restore")]
        [Authorize(Policy = "configuracion.view")]
        public async Task<IActionResult> RestoreOdontogramSymbolConfig(CancellationToken cancellationToken)
        {
            var restored = await _symbols.RestoreDefaultsAsync(null, CurrentUserName(), cancellationToken);
            return NewResult(OkSymbols(restored, "Simbología restaurada a los valores predeterminados."));
        }

        private string CurrentUserName() =>
            User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "sistema";

        private static ApiResponse OkSchedule(ClinicOperatingHours data, string message) => new()
        {
            Succeeded = true,
            StatusCode = HttpStatusCode.OK,
            Data = data,
            Message = message
        };

        private static ApiResponse FailSchedule(string message) => new()
        {
            Succeeded = false,
            StatusCode = HttpStatusCode.BadRequest,
            Message = message
        };

        private static SymbolResponse OkSymbols(OdontogramSymbolConfigDocument data, string message) => new()
        {
            Succeeded = true,
            StatusCode = HttpStatusCode.OK,
            Data = data,
            Message = message
        };

        private static SymbolResponse FailSymbols(string message) => new()
        {
            Succeeded = false,
            StatusCode = HttpStatusCode.BadRequest,
            Message = message
        };

        private ClinicEmailSettings ToPublic(EmailSettings settings) => new()
        {
            Host = settings.Host,
            Port = settings.Port,
            FromEmail = settings.FromEmail,
            SmtpUser = settings.SmtpUser,
            SenderName = settings.SenderName,
            PasswordConfigured = !string.IsNullOrWhiteSpace(settings.Password),
            IsConfigured = _emailSettings.IsConfigured()
        };

        private static string? ValidateEmailSettings(ClinicEmailSettings request, bool allowEmptyPassword)
        {
            if (string.IsNullOrWhiteSpace(request.Host)
                && string.IsNullOrWhiteSpace(request.FromEmail)
                && string.IsNullOrWhiteSpace(request.Password)
                && string.IsNullOrWhiteSpace(request.SmtpUser))
                return null;

            if (string.IsNullOrWhiteSpace(request.Host))
                return "El servidor SMTP es obligatorio.";

            if (request.Port is < 1 or > 65535)
                return "El puerto SMTP no es válido.";

            if (!ContactEmail.TryValidate(request.FromEmail, out _, out var fromError) || string.IsNullOrWhiteSpace(request.FromEmail))
                return fromError ?? "El correo remitente no es válido.";

            if (string.IsNullOrWhiteSpace(request.SenderName))
                return "El nombre del remitente es obligatorio.";

            if (!allowEmptyPassword && string.IsNullOrWhiteSpace(request.Password))
                return "La contraseña SMTP es obligatoria.";

            return null;
        }

        private static EmailSettingsResponse OkEmail(ClinicEmailSettings data, string message) => new()
        {
            Succeeded = true,
            StatusCode = HttpStatusCode.OK,
            Data = data,
            Message = message
        };

        private static EmailSettingsResponse FailEmail(string message) => new()
        {
            Succeeded = false,
            StatusCode = HttpStatusCode.BadRequest,
            Message = message
        };

        private static SendEmailResponse FailSend(string message) => new()
        {
            Succeeded = false,
            StatusCode = HttpStatusCode.BadRequest,
            Message = message
        };

        private static string? ValidateNotifications(PatientNotificationSettings request)
        {
            if (request.DayBeforeEnabled || request.SameDayEnabled)
            {
                if (string.IsNullOrWhiteSpace(request.ReminderSubject) || string.IsNullOrWhiteSpace(request.ReminderBody))
                    return "El asunto y el cuerpo del recordatorio son obligatorios.";
            }

            if (request.BirthdayEnabled
                && (string.IsNullOrWhiteSpace(request.BirthdaySubject) || string.IsNullOrWhiteSpace(request.BirthdayBody)))
                return "El asunto y el cuerpo del cumpleaños son obligatorios si está activo.";

            return null;
        }

        private static NotificationSettingsResponse OkNotifications(PatientNotificationSettings data, string message) => new()
        {
            Succeeded = true,
            StatusCode = HttpStatusCode.OK,
            Data = data,
            Message = message
        };

        private static NotificationSettingsResponse FailNotifications(string message) => new()
        {
            Succeeded = false,
            StatusCode = HttpStatusCode.BadRequest,
            Message = message
        };
    }
}
