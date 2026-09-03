using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clinic_System.Application.Service.Interface;
using Clinic_System.Core.Odontogram;

namespace Clinic_System.API.Services;

public sealed class FileOdontogramSymbolConfigService : IOdontogramSymbolConfigService
{
    private const int HistoryLimit = 40;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly object _gate = new();
    private readonly string _jsonPath;
    private OdontogramSymbolConfigDocument? _cached;

    public FileOdontogramSymbolConfigService(IWebHostEnvironment environment)
    {
        _jsonPath = Path.Combine(environment.ContentRootPath, "App_Data", "odontogram-symbol-config.json");
    }

    public Task<OdontogramSymbolConfigDocument> GetAsync(string? clinicKey = null, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(Clone(EnsureLoaded(clinicKey)));
        }
    }

    public Task<OdontogramSymbolConfigDocument> SaveAsync(
        OdontogramSymbolConfigDocument document,
        string? updatedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        lock (_gate)
        {
            var clinicKey = NormalizeClinicKey(document.ClinicKey);
            var current = EnsureLoaded(clinicKey);
            var incoming = OdontogramSymbolDefaults.Merge(document);
            incoming.ClinicKey = clinicKey;

            var error = OdontogramSymbolDefaults.Validate(incoming);
            if (error != null)
                throw new InvalidOperationException(error);

            Persist(current, incoming, updatedBy, "save");
            return Task.FromResult(Clone(_cached!));
        }
    }

    public Task<OdontogramSymbolConfigDocument> RestoreDefaultsAsync(
        string? clinicKey,
        string? updatedBy,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var key = NormalizeClinicKey(clinicKey);
            var current = EnsureLoaded(key);
            var restored = OdontogramSymbolDefaults.Create(key);
            Persist(current, restored, updatedBy, "restore");
            return Task.FromResult(Clone(_cached!));
        }
    }

    private OdontogramSymbolConfigDocument EnsureLoaded(string? clinicKey)
    {
        var key = NormalizeClinicKey(clinicKey);
        if (_cached != null && string.Equals(_cached.ClinicKey, key, StringComparison.OrdinalIgnoreCase))
            return _cached;

        if (File.Exists(_jsonPath))
        {
            try
            {
                var json = File.ReadAllText(_jsonPath);
                var stored = JsonSerializer.Deserialize<OdontogramSymbolConfigDocument>(json, JsonOptions);
                _cached = OdontogramSymbolDefaults.Merge(stored);
                _cached.ClinicKey = key;
                return _cached;
            }
            catch (JsonException)
            {
            }
        }

        _cached = OdontogramSymbolDefaults.Create(key);
        return _cached;
    }

    private void Persist(
        OdontogramSymbolConfigDocument previous,
        OdontogramSymbolConfigDocument next,
        string? updatedBy,
        string action)
    {
        next.UpdatedAt = DateTimeOffset.UtcNow;
        next.UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "sistema" : updatedBy.Trim();
        next.History = BuildHistory(previous, next, action);

        Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath)!);
        var json = JsonSerializer.Serialize(next, JsonOptions);
        var temp = _jsonPath + ".tmp";
        File.WriteAllText(temp, json, Encoding.UTF8);
        File.Move(temp, _jsonPath, overwrite: true);
        _cached = next;
    }

    private static List<OdontogramSymbolConfigChange> BuildHistory(
        OdontogramSymbolConfigDocument previous,
        OdontogramSymbolConfigDocument next,
        string action)
    {
        var history = previous.History?.Select(CloneChange).ToList() ?? [];
        history.Insert(0, new OdontogramSymbolConfigChange
        {
            At = next.UpdatedAt ?? DateTimeOffset.UtcNow,
            User = next.UpdatedBy ?? "sistema",
            ClinicKey = next.ClinicKey,
            Action = action,
            PreviousConditions = previous.Conditions.Select(x => x.Clone()).ToList(),
            NewConditions = next.Conditions.Select(x => x.Clone()).ToList(),
            PreviousPhases = previous.Phases.Select(x => x.Clone()).ToList(),
            NewPhases = next.Phases.Select(x => x.Clone()).ToList()
        });

        if (history.Count > HistoryLimit)
            history.RemoveRange(HistoryLimit, history.Count - HistoryLimit);

        return history;
    }

    private static string NormalizeClinicKey(string? clinicKey) =>
        string.IsNullOrWhiteSpace(clinicKey)
            ? OdontogramSymbolConfigDocument.DefaultClinicKey
            : clinicKey.Trim();

    private static OdontogramSymbolConfigDocument Clone(OdontogramSymbolConfigDocument source) =>
        JsonSerializer.Deserialize<OdontogramSymbolConfigDocument>(
            JsonSerializer.Serialize(source, JsonOptions), JsonOptions)
        ?? OdontogramSymbolDefaults.Create(source.ClinicKey);

    private static OdontogramSymbolConfigChange CloneChange(OdontogramSymbolConfigChange source) => new()
    {
        At = source.At,
        User = source.User,
        ClinicKey = source.ClinicKey,
        Action = source.Action,
        PreviousConditions = source.PreviousConditions.Select(x => x.Clone()).ToList(),
        NewConditions = source.NewConditions.Select(x => x.Clone()).ToList(),
        PreviousPhases = source.PreviousPhases.Select(x => x.Clone()).ToList(),
        NewPhases = source.NewPhases.Select(x => x.Clone()).ToList()
    };
}
