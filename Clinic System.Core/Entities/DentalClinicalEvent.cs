namespace Clinic_System.Core.Entities;

public class DentalClinicalEvent
{
    public virtual long Id { get; set; }
    public virtual int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;
    public virtual int? ToothNumber { get; set; }
    public virtual DentalClinicalEventType Type { get; set; }
    public virtual ToothChartPhase? Phase { get; set; }
    public virtual string Title { get; set; } = null!;
    public virtual string? Description { get; set; }
    public virtual string? ReferenceType { get; set; }
    public virtual string? ReferenceId { get; set; }
    public virtual string? RecordedByUserId { get; set; }
    public virtual DateTime RecordedAt { get; set; }
}
