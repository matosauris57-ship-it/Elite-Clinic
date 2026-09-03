namespace Clinic_System.Core.Enums;

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
