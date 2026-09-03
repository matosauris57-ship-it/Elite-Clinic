using System.Text;
using System.Text.Json;
using Clinic_System.Application.Common;
using Clinic_System.Application.Service.Interface;

namespace Clinic_System.API.Services
{
    public class FilePatientNotificationSettingsService : IPatientNotificationSettingsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly object _gate = new();
        private readonly string _jsonPath;
        private PatientNotificationSettings? _cached;

        public FilePatientNotificationSettingsService(IWebHostEnvironment environment)
        {
            _jsonPath = Path.Combine(environment.ContentRootPath, "App_Data", "patient-notification-settings.json");
        }

        public PatientNotificationSettings Get()
        {
            lock (_gate)
            {
                return LoadLocked().Normalize();
            }
        }

        public Task SaveAsync(PatientNotificationSettings settings, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(settings);
            var normalized = settings.Normalize();

            lock (_gate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath)!);
                var json = JsonSerializer.Serialize(normalized, JsonOptions);
                var temp = _jsonPath + ".tmp";
                File.WriteAllText(temp, json, Encoding.UTF8);
                File.Move(temp, _jsonPath, overwrite: true);
                _cached = normalized;
            }

            return Task.CompletedTask;
        }

        private PatientNotificationSettings LoadLocked()
        {
            if (_cached != null)
                return _cached;

            if (File.Exists(_jsonPath))
            {
                try
                {
                    var json = File.ReadAllText(_jsonPath);
                    _cached = JsonSerializer.Deserialize<PatientNotificationSettings>(json, JsonOptions)
                        ?? new PatientNotificationSettings();
                    return _cached;
                }
                catch (JsonException)
                {
                }
            }

            _cached = new PatientNotificationSettings();
            return _cached;
        }
    }
}
