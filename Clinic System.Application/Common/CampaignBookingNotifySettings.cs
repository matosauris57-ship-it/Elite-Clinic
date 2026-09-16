using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Common
{
    public class CampaignBookingNotifySettings
    {
        public List<string> Recipients { get; set; } = [];

        public CampaignBookingNotifySettings Normalize()
        {
            var recipients = (Recipients ?? [])
                .SelectMany(SplitRecipients)
                .Select(r => r.Trim().ToLowerInvariant())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new CampaignBookingNotifySettings { Recipients = recipients };
        }

        public static IEnumerable<string> SplitRecipients(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;

            foreach (var part in value.Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                yield return part;
        }

        public static string? Validate(CampaignBookingNotifySettings settings)
        {
            var normalized = settings.Normalize();
            if (normalized.Recipients.Count == 0)
                return "Agregue al menos un correo que recibirá las solicitudes de cita de campaña.";

            foreach (var email in normalized.Recipients)
            {
                if (!ContactEmail.TryValidate(email, out _, out var error) || string.IsNullOrWhiteSpace(email))
                    return error ?? $"Correo inválido: {email}";
            }

            if (normalized.Recipients.Count > 20)
                return "Máximo 20 correos destinatarios.";

            return null;
        }
    }
}
