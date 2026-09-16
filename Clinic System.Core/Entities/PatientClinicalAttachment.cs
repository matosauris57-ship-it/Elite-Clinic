namespace Clinic_System.Core.Entities;

public class PatientClinicalAttachment : ISoftDelete, IAuditable
{
    public virtual int Id { get; set; }
    public virtual int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;
    public virtual ClinicalAttachmentKind Kind { get; set; }
    public virtual string Title { get; set; } = null!;
    public virtual string? Subtype { get; set; }
    public virtual string? Notes { get; set; }
    public virtual int? ToothNumber { get; set; }
    public virtual DateTime? CapturedOn { get; set; }
    public virtual string OriginalFileName { get; set; } = null!;
    public virtual string StoredFileName { get; set; } = null!;
    public virtual string ContentType { get; set; } = null!;
    public virtual long FileSizeBytes { get; set; }
    public virtual string? RecordedByUserId { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
