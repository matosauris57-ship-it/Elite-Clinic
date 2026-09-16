using Clinic_System.Core.Enums;

namespace DentalCare.Admin.Models;

public class InformedConsentTemplateSettings
{
    public Dictionary<string, string> Bodies { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public InformedConsentTemplateSettings Clone()
    {
        return new InformedConsentTemplateSettings
        {
            Bodies = new Dictionary<string, string>(Bodies, StringComparer.OrdinalIgnoreCase)
        };
    }
}

public class InformedConsentTemplateItem
{
    public InformedConsentType Type { get; set; }
    public string Title { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string BodyText { get; set; } = "";
}

public class PatientInformedConsentItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public InformedConsentType ConsentType { get; set; }
    public string ConsentTypeLabel { get; set; } = "";
    public string Title { get; set; } = "";
    public string BodyText { get; set; } = "";
    public string ProcedureExplanation { get; set; } = "";
    public string Benefits { get; set; } = "";
    public string Risks { get; set; } = "";
    public string Alternatives { get; set; } = "";
    public string AuthorizationText { get; set; } = "";
    public string? Notes { get; set; }
    public int? ToothNumber { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorSignatureImageUrl { get; set; }
    public DateTime SignedOn { get; set; }
    public string DateDisplay { get; set; } = "";
    public string OriginalFileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsImage { get; set; }
    public bool IsPdf { get; set; }

    public string DisplayBody =>
        !string.IsNullOrWhiteSpace(BodyText)
            ? BodyText
            : string.Join("\n\n", new[]
            {
                ProcedureExplanation,
                Benefits,
                Risks,
                Alternatives,
                AuthorizationText
            }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

public class UpdateInformedConsentForm
{
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public bool ClearNotes { get; set; }
    public int? ToothNumber { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? SignedOn { get; set; }
}
