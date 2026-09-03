using System.Text;
using System.Text.Json;
using Clinic_System.Application.Common;
using Clinic_System.Application.Service.Interface;
using Clinic_System.Core.Validation;
using Microsoft.Extensions.Options;

namespace Clinic_System.API.Services
{
    public class FileClinicEmailSettingsService : IEmailSettingsProvider
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly IWebHostEnvironment _environment;
        private readonly EmailSettings _defaults;
        private readonly object _gate = new();
        private readonly string _jsonPath;
        private EmailSettings? _cached;

        public FileClinicEmailSettingsService(IWebHostEnvironment environment, IOptions<EmailSettings> emailSettings)
        {
            _environment = environment;
            _defaults = emailSettings.Value;
            _jsonPath = Path.Combine(_environment.ContentRootPath, "App_Data", "clinic-email-settings.json");
        }

        public EmailSettings Get()
        {
            lock (_gate)
            {
                return Clone(Merge(LoadLocked(), _defaults));
            }
        }

        public bool IsConfigured()
        {
            var settings = Get();
            return !string.IsNullOrWhiteSpace(settings.Host)
                && settings.Port is > 0 and < 65536
                && !string.IsNullOrWhiteSpace(settings.FromEmail)
                && !string.IsNullOrWhiteSpace(settings.Password);
        }

        public Task SaveAsync(EmailSettings settings, bool keepExistingPassword = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(settings);

            lock (_gate)
            {
                var current = LoadLocked();
                var password = keepExistingPassword || string.IsNullOrWhiteSpace(settings.Password)
                    ? current.Password
                    : settings.Password;

                var stored = Normalize(new EmailSettings
                {
                    Host = settings.Host,
                    Port = settings.Port,
                    FromEmail = settings.FromEmail,
                    SmtpUser = settings.SmtpUser,
                    Password = password,
                    SenderName = settings.SenderName
                });

                Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath)!);
                var json = JsonSerializer.Serialize(stored, JsonOptions);
                var temp = _jsonPath + ".tmp";
                File.WriteAllText(temp, json, Encoding.UTF8);
                File.Move(temp, _jsonPath, overwrite: true);
                _cached = stored;
            }

            return Task.CompletedTask;
        }

        private EmailSettings LoadLocked()
        {
            if (_cached != null)
                return _cached;

            if (File.Exists(_jsonPath))
            {
                try
                {
                    var json = File.ReadAllText(_jsonPath);
                    _cached = Normalize(JsonSerializer.Deserialize<EmailSettings>(json, JsonOptions) ?? new EmailSettings());
                    return _cached;
                }
                catch (JsonException)
                {
                }
            }

            _cached = new EmailSettings();
            return _cached;
        }

        private static EmailSettings Merge(EmailSettings stored, EmailSettings defaults)
        {
            return Normalize(new EmailSettings
            {
                Host = First(stored.Host, defaults.Host),
                Port = stored.Port > 0 ? stored.Port : (defaults.Port > 0 ? defaults.Port : 587),
                FromEmail = First(stored.FromEmail, defaults.FromEmail),
                SmtpUser = First(stored.SmtpUser, defaults.SmtpUser),
                Password = First(stored.Password, defaults.Password),
                SenderName = First(stored.SenderName, defaults.SenderName)
            });
        }

        private static EmailSettings Normalize(EmailSettings settings)
        {
            var from = ContactEmail.NormalizeOrNull(settings.FromEmail) ?? string.Empty;
            var user = string.IsNullOrWhiteSpace(settings.SmtpUser)
                ? string.Empty
                : settings.SmtpUser.Trim();

            return new EmailSettings
            {
                Host = settings.Host?.Trim() ?? string.Empty,
                Port = settings.Port is < 1 or > 65535 ? 587 : settings.Port,
                FromEmail = from,
                SmtpUser = user,
                Password = settings.Password?.Trim() ?? string.Empty,
                SenderName = string.IsNullOrWhiteSpace(settings.SenderName)
                    ? "Elite Clinic"
                    : settings.SenderName.Trim()
            };
        }

        private static EmailSettings Clone(EmailSettings settings) => new()
        {
            Host = settings.Host,
            Port = settings.Port,
            FromEmail = settings.FromEmail,
            SmtpUser = settings.SmtpUser,
            Password = settings.Password,
            SenderName = settings.SenderName
        };

        private static string First(string? preferred, string? fallback) =>
            string.IsNullOrWhiteSpace(preferred) ? fallback ?? string.Empty : preferred.Trim();
    }
}
