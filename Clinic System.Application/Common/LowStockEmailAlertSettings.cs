using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Common
{
    public class LowStockEmailAlertSettings
    {
        public const string SubjectDefault = "Alerta de inventario bajo - {clinica}";
        public const string IntroDefault =
            "Hay materiales con stock en o por debajo del mínimo configurado en {clinica}. Revise el inventario a la mayor brevedad.";

        public bool Enabled { get; set; }
        public List<string> Recipients { get; set; } = [];
        public TimeSpan SendTime { get; set; } = new(8, 0, 0);
        public DateOnly? LastSentDate { get; set; }
        public string Subject { get; set; } = SubjectDefault;
        public string IntroBody { get; set; } = IntroDefault;

        public LowStockEmailAlertSettings Normalize()
        {
            var recipients = (Recipients ?? [])
                .SelectMany(SplitRecipients)
                .Select(r => r.Trim().ToLowerInvariant())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new LowStockEmailAlertSettings
            {
                Enabled = Enabled,
                Recipients = recipients,
                SendTime = ClampTime(SendTime, new TimeSpan(8, 0, 0)),
                LastSentDate = LastSentDate,
                Subject = First(Subject, SubjectDefault),
                IntroBody = First(IntroBody, IntroDefault)
            };
        }

        public bool ShouldSend(DateTime now) =>
            Enabled
            && Recipients.Count > 0
            && now.TimeOfDay >= SendTime
            && LastSentDate != DateOnly.FromDateTime(now.Date);

        public static string Apply(string template, string clinicName, int lowStockCount) =>
            template
                .Replace("{clinica}", clinicName, StringComparison.OrdinalIgnoreCase)
                .Replace("{cantidad}", lowStockCount.ToString(), StringComparison.OrdinalIgnoreCase);

        public static IEnumerable<string> SplitRecipients(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;

            foreach (var part in value.Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                yield return part;
        }

        public static string? Validate(LowStockEmailAlertSettings settings)
        {
            var normalized = settings.Normalize();
            if (normalized.Enabled && normalized.Recipients.Count == 0)
                return "Agregue al menos un correo destinatario si las alertas están activas.";

            foreach (var email in normalized.Recipients)
            {
                if (!ContactEmail.TryValidate(email, out _, out var error) || string.IsNullOrWhiteSpace(email))
                    return error ?? $"Correo inválido: {email}";
            }

            if (normalized.Recipients.Count > 20)
                return "Máximo 20 correos destinatarios.";

            return null;
        }

        private static TimeSpan ClampTime(TimeSpan value, TimeSpan fallback)
        {
            if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1))
                return fallback;
            return new TimeSpan(value.Hours, value.Minutes, 0);
        }

        private static string First(string? value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
