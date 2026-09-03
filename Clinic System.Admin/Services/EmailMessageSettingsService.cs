using System.Globalization;
using System.Text;
using System.Text.Json;
using Clinic_System.Core.Validation;
using DentalCare.Admin.Models;

namespace DentalCare.Admin.Services;

public class EmailMessageSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly CultureInfo MessageCulture = CultureInfo.GetCultureInfo("es-ES");

    private readonly IWebHostEnvironment _environment;
    private readonly ClinicProfileService _clinicProfile;
    private readonly object _gate = new();
    private readonly string _path;
    private EmailMessageSettings? _cached;

    public EmailMessageSettingsService(IWebHostEnvironment environment, ClinicProfileService clinicProfile)
    {
        _environment = environment;
        _clinicProfile = clinicProfile;
        _path = Path.Combine(_environment.ContentRootPath, "App_Data", "email-message-settings.json");
    }

    public EmailMessageSettings Get()
    {
        lock (_gate)
        {
            return LoadLocked().Clone();
        }
    }

    public void Save(EmailMessageSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var normalized = Normalize(settings);

        lock (_gate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(normalized, JsonOptions);
            var temp = _path + ".tmp";
            File.WriteAllText(temp, json, Encoding.UTF8);
            File.Move(temp, _path, overwrite: true);
            _cached = normalized;
        }
    }

    public string Preview(string template) => ApplyPlaceholders(template, ClinicName(), SampleContext());

    public EmailComposeResult TryCompose(EmailMessageKind kind, WhatsAppAppointmentContext context)
    {
        if (!ContactEmail.TryValidate(context.PatientEmail, out var to, out var emailError) || string.IsNullOrWhiteSpace(to))
            return new EmailComposeResult(false, null, string.Empty, string.Empty,
                emailError ?? "El paciente no tiene un correo válido. Agréguelo en la ficha del paciente.");

        var clinic = _clinicProfile.Get();
        if (!clinic.IsSmtpConfigured)
            return new EmailComposeResult(false, to, string.Empty, string.Empty,
                "Configure el SMTP en Configuración → Datos de la clínica.");

        var settings = Get();
        var subjectTemplate = kind == EmailMessageKind.Confirmation
            ? settings.ConfirmationSubject
            : settings.ReminderSubject;
        var bodyTemplate = kind == EmailMessageKind.Confirmation
            ? settings.ConfirmationTemplate
            : settings.ReminderTemplate;

        var clinicName = ClinicName();
        var subject = ApplyPlaceholders(subjectTemplate, clinicName, context);
        var body = ApplyPlaceholders(bodyTemplate, clinicName, context);

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            return new EmailComposeResult(false, to, string.Empty, string.Empty,
                "La plantilla de correo está vacía. Configúrela en Configuración → Mensajes de correo.");

        return new EmailComposeResult(true, to, subject, body, null);
    }

    private EmailMessageSettings LoadLocked()
    {
        if (_cached != null)
            return _cached;

        if (File.Exists(_path))
        {
            try
            {
                var json = File.ReadAllText(_path);
                _cached = Normalize(JsonSerializer.Deserialize<EmailMessageSettings>(json, JsonOptions) ?? CreateDefaults());
                return _cached;
            }
            catch (JsonException)
            {
            }
        }

        _cached = CreateDefaults();
        return _cached;
    }

    private static EmailMessageSettings CreateDefaults() => new()
    {
        ReminderSubject = "Recordatorio de cita - {clinica}",
        ConfirmationSubject = "Cita confirmada - {clinica}",
        ReminderTemplate = EmailMessageSettings.ReminderDefault,
        ConfirmationTemplate = EmailMessageSettings.ConfirmationDefault,
        DayBeforeEnabled = true,
        SameDayEnabled = false,
        DayBeforeSendTime = new TimeSpan(9, 0, 0),
        SameDaySendTime = new TimeSpan(8, 0, 0),
        BirthdayEnabled = false,
        BirthdaySendTime = new TimeSpan(8, 0, 0),
        BirthdaySubject = "Feliz cumpleaños - {clinica}",
        BirthdayTemplate = EmailMessageSettings.BirthdayDefault
    };

    private static EmailMessageSettings Normalize(EmailMessageSettings settings) => new()
    {
        ReminderSubject = string.IsNullOrWhiteSpace(settings.ReminderSubject)
            ? "Recordatorio de cita - {clinica}"
            : settings.ReminderSubject.Trim(),
        ConfirmationSubject = string.IsNullOrWhiteSpace(settings.ConfirmationSubject)
            ? "Cita confirmada - {clinica}"
            : settings.ConfirmationSubject.Trim(),
        ReminderTemplate = string.IsNullOrWhiteSpace(settings.ReminderTemplate)
            ? EmailMessageSettings.ReminderDefault
            : settings.ReminderTemplate.Trim(),
        ConfirmationTemplate = string.IsNullOrWhiteSpace(settings.ConfirmationTemplate)
            ? EmailMessageSettings.ConfirmationDefault
            : settings.ConfirmationTemplate.Trim(),
        DayBeforeEnabled = settings.DayBeforeEnabled,
        SameDayEnabled = settings.SameDayEnabled,
        DayBeforeSendTime = Clamp(settings.DayBeforeSendTime, new TimeSpan(9, 0, 0)),
        SameDaySendTime = Clamp(settings.SameDaySendTime, new TimeSpan(8, 0, 0)),
        BirthdayEnabled = settings.BirthdayEnabled,
        BirthdaySendTime = Clamp(settings.BirthdaySendTime, new TimeSpan(8, 0, 0)),
        BirthdaySubject = string.IsNullOrWhiteSpace(settings.BirthdaySubject)
            ? "Feliz cumpleaños - {clinica}"
            : settings.BirthdaySubject.Trim(),
        BirthdayTemplate = string.IsNullOrWhiteSpace(settings.BirthdayTemplate)
            ? EmailMessageSettings.BirthdayDefault
            : settings.BirthdayTemplate.Trim()
    };

    private static TimeSpan Clamp(TimeSpan value, TimeSpan fallback) =>
        value < TimeSpan.Zero || value >= TimeSpan.FromDays(1)
            ? fallback
            : new TimeSpan(value.Hours, value.Minutes, 0);

    private string ClinicName()
    {
        var name = _clinicProfile.Get().Name;
        return string.IsNullOrWhiteSpace(name) ? "DentalCare" : name;
    }

    private static WhatsAppAppointmentContext SampleContext() => new()
    {
        PatientName = "María Pérez",
        DoctorName = "Dr. García",
        AppointmentDateTime = DateTime.Today.AddDays(1).AddHours(10)
    };

    private static string ApplyPlaceholders(string template, string clinicName, WhatsAppAppointmentContext context)
    {
        var date = context.AppointmentDateTime;
        var fecha = date?.ToString("d 'de' MMMM yyyy", MessageCulture) ?? "—";
        var hora = date?.ToString("HH:mm", MessageCulture) ?? "—";

        return template
            .Replace("{nombre}", context.PatientName, StringComparison.OrdinalIgnoreCase)
            .Replace("{clinica}", clinicName, StringComparison.OrdinalIgnoreCase)
            .Replace("{fecha}", fecha, StringComparison.OrdinalIgnoreCase)
            .Replace("{hora}", hora, StringComparison.OrdinalIgnoreCase)
            .Replace("{doctor}", context.DoctorName, StringComparison.OrdinalIgnoreCase)
            .Replace("{edad}", "32", StringComparison.OrdinalIgnoreCase);
    }
}
