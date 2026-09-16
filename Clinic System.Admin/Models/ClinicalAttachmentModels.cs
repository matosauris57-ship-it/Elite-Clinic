using Clinic_System.Core.Enums;

namespace DentalCare.Admin.Models;

public class PatientClinicalAttachmentItem
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public ClinicalAttachmentKind Kind { get; set; }
    public string Title { get; set; } = "";
    public string? Subtype { get; set; }
    public string? Notes { get; set; }
    public int? ToothNumber { get; set; }
    public DateTime? CapturedOn { get; set; }
    public string OriginalFileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    public string DateDisplay { get; set; } = "";
    public bool IsImage { get; set; }
    public bool IsPdf { get; set; }
}

public class UpdateClinicalAttachmentForm
{
    public ClinicalAttachmentKind? Kind { get; set; }
    public string? Title { get; set; }
    public string? Subtype { get; set; }
    public string? Notes { get; set; }
    public int? ToothNumber { get; set; }
    public DateTime? CapturedOn { get; set; }
}
