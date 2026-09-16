using System.Text;
using System.Text.Json;
using Clinic_System.Core.Catalog;
using Clinic_System.Core.Enums;
using DentalCare.Admin.Models;

namespace DentalCare.Admin.Services;

public class InformedConsentTemplateSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _gate = new();
    private readonly string _path;
    private InformedConsentTemplateSettings? _cached;

    public InformedConsentTemplateSettingsService(IWebHostEnvironment environment)
    {
        _path = Path.Combine(environment.ContentRootPath, "App_Data", "informed-consent-templates.json");
    }

    public InformedConsentTemplateSettings Get()
    {
        lock (_gate)
            return LoadLocked().Clone();
    }

    public string GetBody(InformedConsentType type)
    {
        lock (_gate)
        {
            var settings = LoadLocked();
            var key = Key(type);
            if (settings.Bodies.TryGetValue(key, out var body) && !string.IsNullOrWhiteSpace(body))
                return InformedConsentCatalog.NormalizeBody(body);

            return InformedConsentCatalog.NormalizeBody(InformedConsentCatalog.GetOrDefault(type).BodyText);
        }
    }

    public void SaveBody(InformedConsentType type, string body)
    {
        var normalized = InformedConsentCatalog.NormalizeBody(body);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("El texto de la plantilla no puede estar vacío.");

        lock (_gate)
        {
            var settings = LoadLocked().Clone();
            settings.Bodies[Key(type)] = normalized;
            PersistLocked(settings);
        }
    }

    public void Save(InformedConsentTemplateSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        lock (_gate)
            PersistLocked(Normalize(settings));
    }

    public void ResetBody(InformedConsentType type)
    {
        lock (_gate)
        {
            var settings = LoadLocked().Clone();
            settings.Bodies.Remove(Key(type));
            PersistLocked(settings);
        }
    }

    private InformedConsentTemplateSettings LoadLocked()
    {
        if (_cached != null)
            return _cached;

        if (!File.Exists(_path))
        {
            _cached = CreateDefaults();
            return _cached;
        }

        try
        {
            var json = File.ReadAllText(_path, Encoding.UTF8);
            _cached = Normalize(JsonSerializer.Deserialize<InformedConsentTemplateSettings>(json, JsonOptions) ?? CreateDefaults());
        }
        catch
        {
            _cached = CreateDefaults();
        }

        return _cached;
    }

    private void PersistLocked(InformedConsentTemplateSettings settings)
    {
        var normalized = Normalize(settings);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        var temp = _path + ".tmp";
        File.WriteAllText(temp, json, Encoding.UTF8);
        File.Move(temp, _path, overwrite: true);
        _cached = normalized;
    }

    private static InformedConsentTemplateSettings CreateDefaults()
    {
        var settings = new InformedConsentTemplateSettings();
        foreach (var template in InformedConsentCatalog.Templates)
            settings.Bodies[Key(template.Type)] = InformedConsentCatalog.NormalizeBody(template.BodyText);
        return settings;
    }

    private static InformedConsentTemplateSettings Normalize(InformedConsentTemplateSettings settings)
    {
        var result = new InformedConsentTemplateSettings();
        foreach (var template in InformedConsentCatalog.Templates)
        {
            var key = Key(template.Type);
            if (settings.Bodies.TryGetValue(key, out var body) && !string.IsNullOrWhiteSpace(body))
                result.Bodies[key] = InformedConsentCatalog.NormalizeBody(body);
            else
                result.Bodies[key] = InformedConsentCatalog.NormalizeBody(template.BodyText);
        }

        return result;
    }

    private static string Key(InformedConsentType type) => ((int)type).ToString();
}
