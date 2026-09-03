namespace DentalCare.Admin.Models;

public class WhatsAppMessageSettings
{
    public const string ReminderDefault =
        "Hola {nombre}, le recordamos su cita en {clinica} el {fecha} a las {hora} con {doctor}. Si no puede asistir, avísenos por este medio. Gracias.";

    public const string ConfirmationDefault =
        "Hola {nombre}, su cita en {clinica} quedó agendada el {fecha} a las {hora} con {doctor}. Gracias.";

    public string ClinicName { get; set; } = "DentalCare";
    public string DefaultCountryCode { get; set; } = "1";
    public string ReminderTemplate { get; set; } = ReminderDefault;
    public string ConfirmationTemplate { get; set; } = ConfirmationDefault;

    public WhatsAppMessageSettings Clone() => new()
    {
        ClinicName = ClinicName,
        DefaultCountryCode = DefaultCountryCode,
        ReminderTemplate = ReminderTemplate,
        ConfirmationTemplate = ConfirmationTemplate
    };
}

public enum WhatsAppMessageKind
{
    Reminder,
    Confirmation
}

public sealed class WhatsAppAppointmentContext
{
    public string PatientName { get; init; } = string.Empty;
    public string? PatientPhone { get; init; }
    public string? PatientEmail { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public DateTime? AppointmentDateTime { get; init; }
}

public sealed record WhatsAppLinkResult(
    bool Success,
    string? Url,
    string RenderedMessage,
    string? Error);
