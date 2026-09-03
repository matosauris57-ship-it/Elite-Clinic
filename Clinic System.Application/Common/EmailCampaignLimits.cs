namespace Clinic_System.Application.Common;

public static class EmailCampaignLimits
{
    public const int BatchSize = 15;
    public const int SubjectMaxLength = 120;
    public const int BodyMaxLength = 4000;
    public const int NameMaxLength = 80;
    public static readonly TimeSpan SendPause = TimeSpan.FromMilliseconds(800);

    public static string Apply(string template, string clinicName, string patientName) =>
        (template ?? string.Empty)
            .Replace("{nombre}", patientName, StringComparison.OrdinalIgnoreCase)
            .Replace("{clinica}", clinicName, StringComparison.OrdinalIgnoreCase);

    public static string AppendFooter(string body, string clinicName)
    {
        var trimmed = (body ?? string.Empty).Trim();
        return trimmed
            + "\n\n—\n"
            + clinicName
            + "\nEste aviso se envía solo a pacientes de la clínica. "
            + "Si no desea recibir campañas, indíquelo en recepción o en su ficha.";
    }
}
