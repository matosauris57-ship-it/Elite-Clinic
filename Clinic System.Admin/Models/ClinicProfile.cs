namespace DentalCare.Admin.Models;

public class ClinicProfile
{
    public string Name { get; set; } = "DentalCare";
    public string? LegalName { get; set; }
    public string? Slogan { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string DefaultCountryCode { get; set; } = "1";
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpFromEmail { get; set; } = string.Empty;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SmtpSenderName { get; set; } = string.Empty;
    public TimeSpan OpenTime { get; set; } = new(12, 0, 0);
    public TimeSpan CloseTime { get; set; } = new(22, 0, 0);
    public int SlotDurationMinutes { get; set; } = 15;
    public List<int> WorkingDays { get; set; } = [0, 1, 2, 3, 4, 5, 6];
    public string? LogoUrl { get; set; }
    public long LogoVersion { get; set; }

    public bool IsOpenOn(DateTime date) =>
        WorkingDays.Contains((int)date.DayOfWeek);

    public string DisplayLogoUrl =>
        string.IsNullOrWhiteSpace(LogoUrl)
            ? string.Empty
            : LogoVersion > 0
                ? $"{LogoUrl}?v={LogoVersion}"
                : LogoUrl;

    public bool IsSmtpConfigured =>
        !string.IsNullOrWhiteSpace(SmtpHost)
        && SmtpPort is > 0 and < 65536
        && !string.IsNullOrWhiteSpace(SmtpFromEmail)
        && !string.IsNullOrWhiteSpace(SmtpPassword);

    public ClinicProfile Clone() => new()
    {
        Name = Name,
        LegalName = LegalName,
        Slogan = Slogan,
        Address = Address,
        Phone = Phone,
        Email = Email,
        Website = Website,
        TaxId = TaxId,
        DefaultCountryCode = DefaultCountryCode,
        SmtpHost = SmtpHost,
        SmtpPort = SmtpPort,
        SmtpFromEmail = SmtpFromEmail,
        SmtpUser = SmtpUser,
        SmtpPassword = SmtpPassword,
        SmtpSenderName = SmtpSenderName,
        OpenTime = OpenTime,
        CloseTime = CloseTime,
        SlotDurationMinutes = SlotDurationMinutes,
        WorkingDays = [.. WorkingDays],
        LogoUrl = LogoUrl,
        LogoVersion = LogoVersion
    };
}
