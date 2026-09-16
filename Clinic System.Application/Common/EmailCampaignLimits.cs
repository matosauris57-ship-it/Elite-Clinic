using Clinic_System.Core.Messaging;

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
        MessageBodyFormatting.ApplyTokens(template ?? string.Empty, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["{nombre}"] = patientName,
            ["{clinica}"] = clinicName,
            ["{enlace_cita}"] = bookingUrl ?? string.Empty
        });

    public static string AppendFooter(string body, string clinicName, string? bookingUrl = null)
    {
        var trimmed = (body ?? string.Empty).Trim();
        var bookingNote = string.Empty;
        if (!string.IsNullOrWhiteSpace(bookingUrl))
        {
            var alreadyIncludesLink = trimmed.Contains(bookingUrl, StringComparison.OrdinalIgnoreCase)
                || trimmed.Contains("{enlace_cita}", StringComparison.OrdinalIgnoreCase);
            if (!alreadyIncludesLink)
            {
                bookingNote = "Para agendar su cita en el horario que más le convenga (cualquier día y hora disponible):\n"
                    + bookingUrl.Trim();
            }
        }

        if (MessageBodyFormatting.LooksLikeHtml(trimmed))
        {
            var extra = string.IsNullOrWhiteSpace(bookingNote)
                ? string.Empty
                : "<br>" + System.Net.WebUtility.HtmlEncode(bookingNote).Replace("\n", "<br>");

            return trimmed
                + "<p style=\"margin-top:1.2em;color:#666;font-size:13px\">—<br>"
                + System.Net.WebUtility.HtmlEncode(clinicName)
                + extra
                + "<br>Este aviso se envía solo a pacientes de la clínica. "
                + "Si no desea recibir campañas, indíquelo en recepción o en su ficha.</p>";
        }

        var footer = "\n\n—\n" + clinicName;
        if (!string.IsNullOrWhiteSpace(bookingNote))
            footer += "\n\n" + bookingNote;
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
