using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class WhatsAppMessageSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly CultureInfo MessageCulture = CultureInfo.GetCultureInfo("es-ES");
    private static readonly Regex NonDigit = new(@"\D", RegexOptions.Compiled);

    private readonly IWebHostEnvironment _environment;
    private readonly ClinicProfileService _clinicProfile;
    private readonly ClinicSettings _defaults;
    private readonly object _gate = new();
    private readonly string _path;
    private WhatsAppMessageSettings? _cached;

    public WhatsAppMessageSettingsService(
        IWebHostEnvironment environment,
        ClinicProfileService clinicProfile,
        IOptions<ClinicSettings> clinicSettings)
    {
        _environment = environment;
        _clinicProfile = clinicProfile;
        _defaults = clinicSettings.Value;
        _path = Path.Combine(_environment.ContentRootPath, "App_Data", "whatsapp-message-settings.json");
    }

    public WhatsAppMessageSettings Get()
    {
        lock (_gate)
        {
            return LoadLocked().Clone();
        }
    }

    public void Save(WhatsAppMessageSettings settings)
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

    public string Render(WhatsAppMessageKind kind, WhatsAppAppointmentContext context)
    {
        var settings = Get();
        var template = kind == WhatsAppMessageKind.Confirmation
            ? settings.ConfirmationTemplate
            : settings.ReminderTemplate;

        return ApplyPlaceholders(template, ClinicName(), context);
    }

    public string Preview(string template, string? clinicName = null)
    {
        var sample = new WhatsAppAppointmentContext
        {
            PatientName = "María Pérez",
            DoctorName = "Dr. García",
            AppointmentDateTime = DateTime.Today.AddDays(1).AddHours(10)
        };

        return ApplyPlaceholders(template, string.IsNullOrWhiteSpace(clinicName) ? ClinicName() : clinicName, sample);
    }

    public WhatsAppLinkResult TryBuildLink(WhatsAppMessageKind kind, WhatsAppAppointmentContext context)
    {
        var settings = Get();
        if (!TryNormalizePhone(context.PatientPhone, ClinicCountryCode(), out var digits, out var phoneError))
            return new WhatsAppLinkResult(false, null, string.Empty, phoneError);

        var message = Render(kind, context);
        if (string.IsNullOrWhiteSpace(message))
            return new WhatsAppLinkResult(false, null, string.Empty, "La plantilla del mensaje está vacía. Configúrela en Configuración → Mensajes WhatsApp.");

        var url = $"https://wa.me/{digits}?text={Uri.EscapeDataString(message)}";
        return new WhatsAppLinkResult(true, url, message, null);
    }

    public static bool TryNormalizePhone(string? raw, string? countryCode, out string digits, out string? error)
    {
        digits = string.Empty;
        var cleaned = NonDigit.Replace(raw ?? string.Empty, string.Empty);
        if (string.IsNullOrEmpty(cleaned))
        {
            error = "El paciente no tiene un teléfono válido para WhatsApp.";
            return false;
        }

        var code = NonDigit.Replace(countryCode ?? string.Empty, string.Empty);
        if (cleaned.Length < 11 && !string.IsNullOrEmpty(code) && !cleaned.StartsWith(code, StringComparison.Ordinal))
            cleaned = code + cleaned;

        if (cleaned.Length is < 10 or > 15)
        {
            error = "El teléfono debe tener entre 10 y 15 dígitos (incluye el código de país).";
            return false;
        }

        digits = cleaned;
        error = null;
        return true;
    }

    private WhatsAppMessageSettings LoadLocked()
    {
        if (_cached != null)
            return _cached;

        if (File.Exists(_path))
        {
            try
            {
                var json = File.ReadAllText(_path);
                _cached = Normalize(JsonSerializer.Deserialize<WhatsAppMessageSettings>(json, JsonOptions) ?? CreateDefaults());
                return _cached;
            }
            catch (JsonException)
            {
                // Archivo corrupto: volver a defaults en memoria.
            }
        }

        _cached = CreateDefaults();
        return _cached;
    }

    private string ClinicName()
    {
        var name = _clinicProfile.Get().Name;
        return string.IsNullOrWhiteSpace(name) ? "DentalCare" : name;
    }

    private string ClinicCountryCode()
    {
        var code = _clinicProfile.Get().DefaultCountryCode;
        return string.IsNullOrWhiteSpace(code) ? "1" : code;
    }

    private WhatsAppMessageSettings CreateDefaults() => new()
    {
        ClinicName = string.IsNullOrWhiteSpace(_defaults.Name) ? "DentalCare" : _defaults.Name.Trim(),
        DefaultCountryCode = string.IsNullOrWhiteSpace(_defaults.DefaultCountryCode)
            ? "1"
            : NonDigit.Replace(_defaults.DefaultCountryCode, string.Empty),
        ReminderTemplate = WhatsAppMessageSettings.ReminderDefault,
        ConfirmationTemplate = WhatsAppMessageSettings.ConfirmationDefault
    };

    private static WhatsAppMessageSettings Normalize(WhatsAppMessageSettings settings)
    {
        var code = NonDigit.Replace(settings.DefaultCountryCode ?? string.Empty, string.Empty);
        if (string.IsNullOrEmpty(code))
            code = "1";

        return new WhatsAppMessageSettings
        {
            ClinicName = string.IsNullOrWhiteSpace(settings.ClinicName) ? "DentalCare" : settings.ClinicName.Trim(),
            DefaultCountryCode = code,
            ReminderTemplate = string.IsNullOrWhiteSpace(settings.ReminderTemplate)
                ? WhatsAppMessageSettings.ReminderDefault
                : settings.ReminderTemplate.Trim(),
            ConfirmationTemplate = string.IsNullOrWhiteSpace(settings.ConfirmationTemplate)
                ? WhatsAppMessageSettings.ConfirmationDefault
                : settings.ConfirmationTemplate.Trim()
        };
    }

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
            .Replace("{doctor}", context.DoctorName, StringComparison.OrdinalIgnoreCase);
    }
}
