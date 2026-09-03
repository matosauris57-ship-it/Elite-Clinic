namespace Clinic_System.Core.Entities;

public class PatientPrescription : ISoftDelete, IAuditable
{
    public virtual int Id { get; set; }
    public virtual int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;
    public virtual int? DoctorId { get; set; }
    public virtual Doctor? Doctor { get; set; }
    public virtual DateTime IssuedAt { get; set; }
    public virtual string? Diagnosis { get; set; }
    public virtual string? Notes { get; set; }
    public virtual string? RecordedByUserId { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual ICollection<PatientPrescriptionItem> Items { get; set; } = new List<PatientPrescriptionItem>();

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
    }
}

public class PatientPrescriptionItem
{
    public virtual int Id { get; set; }
    public virtual int PatientPrescriptionId { get; set; }
    public virtual PatientPrescription Prescription { get; set; } = null!;
    public virtual int SortOrder { get; set; }
    public virtual string? TemplateKey { get; set; }
    public virtual string MedicationName { get; set; } = null!;
    public virtual string Dosage { get; set; } = null!;
    public virtual string Frequency { get; set; } = null!;
    public virtual int DurationDays { get; set; }
    public virtual string? SpecialInstructions { get; set; }
}
