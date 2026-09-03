namespace DentalCare.Admin.Models;

public class EmailMessageSettings
{
    public const string ReminderDefault =
        "Hola {nombre}, le recordamos su cita en {clinica} el {fecha} a las {hora} con {doctor}. Si no puede asistir, avísenos. Gracias.";

    public const string ConfirmationDefault =
        "Hola {nombre}, su cita en {clinica} quedó agendada el {fecha} a las {hora} con {doctor}. Gracias.";

    public const string BirthdayDefault =
        "Hola {nombre}, en {clinica} le deseamos un muy feliz cumpleaños. ¡Que cumpla {edad} años llenos de salud y sonrisas!";

    public string ReminderSubject { get; set; } = "Recordatorio de cita - {clinica}";
    public string ConfirmationSubject { get; set; } = "Cita confirmada - {clinica}";
    public string ReminderTemplate { get; set; } = ReminderDefault;
    public string ConfirmationTemplate { get; set; } = ConfirmationDefault;
    public bool DayBeforeEnabled { get; set; } = true;
    public bool SameDayEnabled { get; set; }
    public TimeSpan DayBeforeSendTime { get; set; } = new(9, 0, 0);
    public TimeSpan SameDaySendTime { get; set; } = new(8, 0, 0);
    public bool BirthdayEnabled { get; set; }
    public TimeSpan BirthdaySendTime { get; set; } = new(8, 0, 0);
    public string BirthdaySubject { get; set; } = "Feliz cumpleaños - {clinica}";
    public string BirthdayTemplate { get; set; } = BirthdayDefault;

    public EmailMessageSettings Clone() => new()
    {
        ReminderSubject = ReminderSubject,
        ConfirmationSubject = ConfirmationSubject,
        ReminderTemplate = ReminderTemplate,
        ConfirmationTemplate = ConfirmationTemplate,
        DayBeforeEnabled = DayBeforeEnabled,
        SameDayEnabled = SameDayEnabled,
        DayBeforeSendTime = DayBeforeSendTime,
        SameDaySendTime = SameDaySendTime,
        BirthdayEnabled = BirthdayEnabled,
        BirthdaySendTime = BirthdaySendTime,
        BirthdaySubject = BirthdaySubject,
        BirthdayTemplate = BirthdayTemplate
    };

    public PatientNotificationApiModel ToApi() => new()
    {
        DayBeforeEnabled = DayBeforeEnabled,
        SameDayEnabled = SameDayEnabled,
        DayBeforeSendTime = DayBeforeSendTime,
        SameDaySendTime = SameDaySendTime,
        BirthdayEnabled = BirthdayEnabled,
        BirthdaySendTime = BirthdaySendTime,
        ReminderSubject = ReminderSubject,
        ReminderBody = ReminderTemplate,
        BirthdaySubject = BirthdaySubject,
        BirthdayBody = BirthdayTemplate
    };

    public void ApplyFromApi(PatientNotificationApiModel api)
    {
        DayBeforeEnabled = api.DayBeforeEnabled;
        SameDayEnabled = api.SameDayEnabled;
        DayBeforeSendTime = api.DayBeforeSendTime;
        SameDaySendTime = api.SameDaySendTime;
        BirthdayEnabled = api.BirthdayEnabled;
        BirthdaySendTime = api.BirthdaySendTime;
        if (!string.IsNullOrWhiteSpace(api.ReminderSubject))
            ReminderSubject = api.ReminderSubject;
        if (!string.IsNullOrWhiteSpace(api.ReminderBody))
            ReminderTemplate = api.ReminderBody;
        if (!string.IsNullOrWhiteSpace(api.BirthdaySubject))
            BirthdaySubject = api.BirthdaySubject;
        if (!string.IsNullOrWhiteSpace(api.BirthdayBody))
            BirthdayTemplate = api.BirthdayBody;
    }
}

public class PatientNotificationApiModel
{
    public bool DayBeforeEnabled { get; set; }
    public bool SameDayEnabled { get; set; }
    public TimeSpan DayBeforeSendTime { get; set; }
    public TimeSpan SameDaySendTime { get; set; }
    public bool BirthdayEnabled { get; set; }
    public TimeSpan BirthdaySendTime { get; set; }
    public string ReminderSubject { get; set; } = string.Empty;
    public string ReminderBody { get; set; } = string.Empty;
    public string BirthdaySubject { get; set; } = string.Empty;
    public string BirthdayBody { get; set; } = string.Empty;
}

public enum EmailMessageKind
{
    Reminder,
    Confirmation,
    Birthday
}

public sealed record EmailComposeResult(
    bool Success,
    string? To,
    string Subject,
    string Body,
    string? Error);
