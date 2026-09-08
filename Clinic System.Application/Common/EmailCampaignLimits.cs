namespace Clinic_System.Application.Common;

public static class EmailCampaignLimits
{
    public const int BatchSize = 15;
    public const int SubjectMaxLength = 120;
    public const int BodyMaxLength = 4000;
    public const int NameMaxLength = 80;
    public static readonly TimeSpan SendPause = TimeSpan.FromMilliseconds(800);
    public static readonly TimeSpan BookingTokenLifetime = TimeSpan.FromDays(30);

    public static string Apply(string template, string clinicName, string patientName, string? bookingUrl = null) =>
        (template ?? string.Empty)
            .Replace("{nombre}", patientName, StringComparison.OrdinalIgnoreCase)
            .Replace("{clinica}", clinicName, StringComparison.OrdinalIgnoreCase)
            .Replace("{enlace_cita}", bookingUrl ?? string.Empty, StringComparison.OrdinalIgnoreCase);

    public static string AppendFooter(string body, string clinicName, string? bookingUrl = null)
    {
        var trimmed = (body ?? string.Empty).Trim();
        var footer = "\n\n—\n" + clinicName;

        if (!string.IsNullOrWhiteSpace(bookingUrl))
        {
            var alreadyIncludesLink = trimmed.Contains(bookingUrl, StringComparison.OrdinalIgnoreCase)
                || trimmed.Contains("{enlace_cita}", StringComparison.OrdinalIgnoreCase);
            if (!alreadyIncludesLink)
            {
                footer += "\n\nPara agendar su cita en el horario que más le convenga (cualquier día y hora disponible):\n"
                    + bookingUrl.Trim();
            }
        }

        footer += "\nEste aviso se envía solo a pacientes de la clínica. "
            + "Si no desea recibir campañas, indíquelo en recepción o en su ficha.";

        return trimmed + footer;
    }

    public static string? BuildBookingUrl(string? publicSiteUrl, string token)
    {
        if (string.IsNullOrWhiteSpace(publicSiteUrl) || string.IsNullOrWhiteSpace(token))
            return null;

        var baseUrl = publicSiteUrl.Trim().TrimEnd('/');
        return $"{baseUrl}/agendar-campana?token={Uri.EscapeDataString(token)}";
    }
}
