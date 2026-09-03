using System.Text;
using System.Text.Json;
using Clinic_System.Application.Common;
using Clinic_System.Application.Service.Interface;
using Microsoft.Extensions.Options;

namespace Clinic_System.API.Services
{
    public class FileClinicOperatingHoursService : IClinicOperatingHoursService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly IWebHostEnvironment _environment;
        private readonly ClinicSettings _defaults;
        private readonly object _gate = new();
        private readonly string _jsonPath;
        private ClinicOperatingHours? _cached;

        public FileClinicOperatingHoursService(IWebHostEnvironment environment, IOptions<ClinicSettings> clinicSettings)
        {
            _environment = environment;
            _defaults = clinicSettings.Value;
            _jsonPath = Path.Combine(_environment.ContentRootPath, "App_Data", "clinic-schedule.json");
        }

        public Task<ClinicOperatingHours> GetAsync(CancellationToken cancellationToken = default)
        {
            lock (_gate)
            {
                return Task.FromResult(LoadLocked().Normalize());
            }
        }

        public Task SaveAsync(ClinicOperatingHours hours, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(hours);
            var normalized = hours.Normalize();

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

        private ClinicOperatingHours LoadLocked()
        {
            if (_cached != null)
                return _cached;

            if (File.Exists(_jsonPath))
            {
                try
                {
                    var json = File.ReadAllText(_jsonPath);
                    _cached = JsonSerializer.Deserialize<ClinicOperatingHours>(json, JsonOptions) ?? CreateDefaults();
                    return _cached;
                }
                catch (JsonException)
                {
                }
            }

            _cached = CreateDefaults();
            return _cached;
        }

        private ClinicOperatingHours CreateDefaults()
        {
            var open = _defaults.DayStartTime == default ? new TimeSpan(12, 0, 0) : _defaults.DayStartTime;
            var close = _defaults.DayEndTime == default ? new TimeSpan(22, 0, 0) : _defaults.DayEndTime;
            var duration = _defaults.SlotDurationInMinutes <= 0 ? 15 : _defaults.SlotDurationInMinutes;

            return new ClinicOperatingHours
            {
                OpenTime = open,
                CloseTime = close,
                SlotDurationMinutes = duration,
                WorkingDays = [0, 1, 2, 3, 4, 5, 6]
            };
        }
    }
}
