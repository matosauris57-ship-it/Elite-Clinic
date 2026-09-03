namespace Clinic_System.Application.Features.ToothChart.Models;

public class GetCurrentToothChartQuery : IRequest<Response<List<ToothChartEntryDTO>>>
{
    public int PatientId { get; set; }
    public string? Dentition { get; set; }
    public int? Quadrant { get; set; }
}

public class GetDentalTimelineQuery : IRequest<Response<List<DentalClinicalEventDTO>>>
{
    public int PatientId { get; set; }
    public int? ToothNumber { get; set; }
}

public class CreateToothChartEntryCommand : IRequest<Response<ToothChartEntryDTO>>
{
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
    public int? AppointmentId { get; set; }
    public List<int> ToothNumbers { get; set; } = [];
    public Guid? BridgeSpanId { get; set; }
    public List<BridgeUnitInput> BridgeUnits { get; set; } = [];
}

public class CreateToothChartEntriesBatchCommand : IRequest<Response<List<ToothChartEntryDTO>>>
{
    public int PatientId { get; set; }
    public List<int> ToothNumbers { get; set; } = [];
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
    public int? AppointmentId { get; set; }
    public Guid? BridgeSpanId { get; set; }
    public List<BridgeUnitInput> BridgeUnits { get; set; } = [];
}

public class BridgeUnitInput
{
    public int ToothNumber { get; set; }
    public BridgeRole Role { get; set; }
}
