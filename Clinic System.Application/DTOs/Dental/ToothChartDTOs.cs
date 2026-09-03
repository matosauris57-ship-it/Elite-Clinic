namespace Clinic_System.Application.DTOs.Dental;

public class ToothChartEntryDTO
{
    public long Id { get; set; }
    public int PatientId { get; set; }
    public int ToothNumber { get; set; }
    public ToothSurface Surface { get; set; }
    public ToothChartPhase Phase { get; set; }
    public ToothCondition Condition { get; set; }
    public RestorationMaterial? RestorationMaterial { get; set; }
    public CariesType? CariesType { get; set; }
    public IcdasCode? Icdas { get; set; }
    public ToothSeverity? Severity { get; set; }
    public string? ClinicalDiagnosis { get; set; }
    public string? ProposedTreatment { get; set; }
    public string? Notes { get; set; }
    public Guid? BridgeSpanId { get; set; }
    public BridgeRole? BridgeRole { get; set; }
    public int? AppointmentId { get; set; }
    public string? RecordedByUserId { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class DentalClinicalEventDTO
{
    public long Id { get; set; }
    public int PatientId { get; set; }
    public int? ToothNumber { get; set; }
    public DentalClinicalEventType Type { get; set; }
    public ToothChartPhase? Phase { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? RecordedByUserId { get; set; }
    public string? RecordedByUserName { get; set; }
    public DateTime RecordedAt { get; set; }
}
