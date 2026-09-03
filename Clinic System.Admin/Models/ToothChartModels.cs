namespace DentalCare.Admin.Models;

public enum ToothSurface
{
    WholeTooth = 0,
    Mesial = 1,
    Distal = 2,
    BuccalFacial = 3,
    LingualPalatal = 4,
    OcclusalIncisal = 5
}

public enum ToothChartPhase
{
    Diagnosis = 0,
    Planned = 1,
    Completed = 2,
    InTreatment = 3
}

public enum ToothFindingScope
{
    WholeTooth = 0,
    Surface = 1
}

public enum CariesType
{
    PitAndFissure = 1,
    SmoothSurface = 2,
    Root = 3,
    Secondary = 4
}

public enum IcdasCode
{
    Sound = 0,
    InitialVisual = 1,
    DistinctVisual = 2,
    LocalizedEnamelBreakdown = 3,
    UnderlyingDarkShadow = 4,
    DistinctCavityWithDentin = 5,
    ExtensiveCavityWithDentin = 6
}

public enum ToothCondition
{
    Healthy = 0,
    Caries = 1,
    Missing = 2,
    Crown = 3,
    Filling = 4,
    RootCanal = 5,
    Implant = 6,
    Fractured = 7,
    Extracted = 8,
    Prosthesis = 9,
    Bridge = 10,
        Sealant = 11,
        Other = 12,
        Mobility = 13
    }

public enum ToothSeverity
{
    Mild = 1,
    Moderate = 2,
    Severe = 3
}

public enum RestorationMaterial
{
    Amalgam = 1,
    Resin = 2,
    Inlay = 3,
    Onlay = 4,
    Veneer = 5,
    Temporary = 6,
    MetalCeramic = 7,
    Porcelain = 8
}

public enum DentalClinicalEventType
{
    OdontogramEntry = 0,
    TreatmentPlan = 1,
    Treatment = 2,
    ClinicalNote = 3,
    PeriodontalExam = 4,
    Prescription = 5
}

public enum BridgeRole
{
    Abutment = 0,
    Pontic = 1
}

public class ToothChartEntry
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

public class DentalClinicalEvent
{
    public long Id { get; set; }
    public int PatientId { get; set; }
    public int? ToothNumber { get; set; }
    public DentalClinicalEventType Type { get; set; }
    public ToothChartPhase? Phase { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? RecordedByUserId { get; set; }
    public string? RecordedByUserName { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class CreateToothChartEntryRequest
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

public class CreateToothChartEntriesBatchRequest
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
