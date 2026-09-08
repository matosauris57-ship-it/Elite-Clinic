namespace Clinic_System.Core.Entities;

public class PatientMedicalCertificate : ISoftDelete, IAuditable
{
    public virtual int Id { get; set; }
    public virtual int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;
    public virtual int? DoctorId { get; set; }
    public virtual Doctor? Doctor { get; set; }
    public virtual DateTime IssuedAt { get; set; }
    public virtual string CertificateType { get; set; } = null!;
    public virtual string Diagnosis { get; set; } = null!;
    public virtual string Recommendation { get; set; } = null!;
    public virtual bool IncludesRest { get; set; }
    public virtual DateTime? RestStartDate { get; set; }
    public virtual DateTime? RestEndDate { get; set; }
    public virtual int? RestDays { get; set; }
    public virtual string? Observations { get; set; }
    public virtual string? Purpose { get; set; }
    public virtual string? RecordedByUserId { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
    }
}
