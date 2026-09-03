using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Clinic_System.Core.Validation;
using DentalCare.Admin.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class ClinicProfileService
{
    public const long MaxLogoBytes = 2 * 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly Regex NonDigit = new(@"\D", RegexOptions.Compiled);

    private static readonly HashSet<string> AllowedLogoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly ClinicSettings _defaults;
    private readonly object _gate = new();
    private readonly string _jsonPath;
    private readonly string _logoDirectory;
    private ClinicProfile? _cached;

    public ClinicProfileService(IWebHostEnvironment environment, IOptions<ClinicSettings> clinicSettings)
    {
        _environment = environment;
        _defaults = clinicSettings.Value;
        _jsonPath = Path.Combine(_environment.ContentRootPath, "App_Data", "clinic-profile.json");
        _logoDirectory = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "clinic");
    }

    public event Action? Changed;

    public ClinicProfile Get()
    {
        lock (_gate)
        {
            return LoadLocked().Clone();
        }
    }

    public void Save(ClinicProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        lock (_gate)
        {
            var current = LoadLocked();
            profile.LogoUrl = current.LogoUrl;
            profile.LogoVersion = current.LogoVersion;
            WriteLocked(Normalize(profile));
        }

        Changed?.Invoke();
    }

    public async Task SaveLogoAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        var extension = Path.GetExtension(file.Name);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedLogoExtensions.Contains(extension))
            throw new InvalidOperationException("El logo debe ser PNG, JPG, WEBP, GIF o SVG.");

        if (file.Size <= 0 || file.Size > MaxLogoBytes)
            throw new InvalidOperationException("El logo no puede superar 2 MB.");

        Directory.CreateDirectory(_logoDirectory);
        var fileName = $"logo{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(_logoDirectory, fileName);

        await using (var stream = file.OpenReadStream(MaxLogoBytes, cancellationToken))
        await using (var output = File.Create(physicalPath))
        {
            await stream.CopyToAsync(output, cancellationToken);
        }

        lock (_gate)
        {
            foreach (var leftover in Directory.EnumerateFiles(_logoDirectory, "logo.*"))
            {
                if (!string.Equals(leftover, physicalPath, StringComparison.OrdinalIgnoreCase))
                    File.Delete(leftover);
            }

            var current = LoadLocked();
            current.LogoUrl = $"/uploads/clinic/{fileName}";
            current.LogoVersion = DateTime.UtcNow.Ticks;
            WriteLocked(Normalize(current));
        }

        Changed?.Invoke();
    }

    public void RemoveLogo()
    {
        lock (_gate)
        {
            if (Directory.Exists(_logoDirectory))
            {
                foreach (var leftover in Directory.EnumerateFiles(_logoDirectory, "logo.*"))
                    File.Delete(leftover);
            }

            var current = LoadLocked();
            current.LogoUrl = null;
            current.LogoVersion = 0;
            WriteLocked(Normalize(current));
        }

        Changed?.Invoke();
    }

    private ClinicProfile LoadLocked()
    {
        if (_cached != null)
            return _cached;

        if (File.Exists(_jsonPath))
        {
            try
            {
                var json = File.ReadAllText(_jsonPath);
                _cached = Normalize(JsonSerializer.Deserialize<ClinicProfile>(json, JsonOptions) ?? CreateDefaults());
                return _cached;
            }
            catch (JsonException)
            {
            }
        }

        _cached = CreateDefaults();
        return _cached;
    }

    private void WriteLocked(ClinicProfile profile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath)!);
        var json = JsonSerializer.Serialize(profile, JsonOptions);
        var temp = _jsonPath + ".tmp";
        File.WriteAllText(temp, json, Encoding.UTF8);
        File.Move(temp, _jsonPath, overwrite: true);
        _cached = profile;
    }

    private ClinicProfile CreateDefaults() => Normalize(new ClinicProfile
    {
        Name = _defaults.Name,
        Address = _defaults.Address,
        Phone = _defaults.Phone,
        TaxId = _defaults.TaxId,
        DefaultCountryCode = _defaults.DefaultCountryCode
    });

    private ClinicProfile Normalize(ClinicProfile settings)
    {
        var code = NonDigit.Replace(settings.DefaultCountryCode ?? string.Empty, string.Empty);
        if (string.IsNullOrEmpty(code))
            code = NonDigit.Replace(_defaults.DefaultCountryCode ?? string.Empty, string.Empty);
        if (string.IsNullOrEmpty(code))
            code = "1";

        var fallbackName = string.IsNullOrWhiteSpace(_defaults.Name) ? "DentalCare" : _defaults.Name.Trim();

        return new ClinicProfile
        {
            Name = string.IsNullOrWhiteSpace(settings.Name) ? fallbackName : settings.Name.Trim(),
            LegalName = TrimOrNull(settings.LegalName),
            Slogan = TrimOrNull(settings.Slogan),
            Address = settings.Address?.Trim() ?? string.Empty,
            Phone = settings.Phone?.Trim() ?? string.Empty,
            Email = settings.Email?.Trim() ?? string.Empty,
            Website = settings.Website?.Trim() ?? string.Empty,
            TaxId = settings.TaxId?.Trim() ?? string.Empty,
            DefaultCountryCode = code,
            SmtpHost = string.IsNullOrWhiteSpace(settings.SmtpHost) ? "smtp.gmail.com" : settings.SmtpHost.Trim(),
            SmtpPort = settings.SmtpPort is < 1 or > 65535 ? 587 : settings.SmtpPort,
            SmtpFromEmail = ContactEmail.NormalizeOrNull(settings.SmtpFromEmail)
                ?? ContactEmail.NormalizeOrNull(settings.Email)
                ?? string.Empty,
            SmtpUser = settings.SmtpUser?.Trim() ?? string.Empty,
            SmtpPassword = settings.SmtpPassword?.Trim() ?? string.Empty,
            SmtpSenderName = string.IsNullOrWhiteSpace(settings.SmtpSenderName)
                ? (string.IsNullOrWhiteSpace(settings.Name) ? fallbackName : settings.Name.Trim())
                : settings.SmtpSenderName.Trim(),
            OpenTime = settings.OpenTime,
            CloseTime = settings.CloseTime,
            SlotDurationMinutes = settings.SlotDurationMinutes is < 5 or > 120 ? 15 : settings.SlotDurationMinutes,
            WorkingDays = NormalizeWorkingDays(settings.WorkingDays),
            LogoUrl = string.IsNullOrWhiteSpace(settings.LogoUrl) ? null : settings.LogoUrl.Trim(),
            LogoVersion = settings.LogoVersion
        };
    }

    private static List<int> NormalizeWorkingDays(List<int>? days)
    {
        var normalized = (days ?? [])
            .Where(d => d is >= 0 and <= 6)
            .Distinct()
            .OrderBy(d => d)
            .ToList();
        return normalized.Count == 0 ? [0, 1, 2, 3, 4, 5, 6] : normalized;
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
