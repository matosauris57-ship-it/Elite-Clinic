namespace Clinic_System.Core.Entities;

public class ToothChartEntry
{
    public virtual long Id { get; set; }
    public virtual int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;
    public virtual int ToothNumber { get; set; }
    public virtual ToothSurface Surface { get; set; }
    public virtual ToothChartPhase Phase { get; set; }
    public virtual ToothCondition Condition { get; set; }
    public virtual RestorationMaterial? RestorationMaterial { get; set; }
    public virtual CariesType? CariesType { get; set; }
    public virtual IcdasCode? Icdas { get; set; }
    public virtual ToothSeverity? Severity { get; set; }
    public virtual string? ClinicalDiagnosis { get; set; }
    public virtual string? ProposedTreatment { get; set; }
    public virtual string? Notes { get; set; }
    public virtual int? AppointmentId { get; set; }
    public virtual Appointment? Appointment { get; set; }
    public virtual Guid? BridgeSpanId { get; set; }
    public virtual BridgeRole? BridgeRole { get; set; }
    public virtual string? RecordedByUserId { get; set; }
    public virtual DateTime RecordedAt { get; set; }
}
