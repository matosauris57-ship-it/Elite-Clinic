namespace DentalCare.Admin.Models;

public enum PeriodontalSurface
{
    Facial = 0,
    Lingual = 1
}

public enum PeriodontalSitePosition
{
    Mesial = 0,
    Center = 1,
    Distal = 2
}

public enum PeriodontalMobility
{
    Grade0 = 0,
    Grade1 = 1,
    Grade2 = 2,
    Grade3 = 3
}

public enum PeriodontalFurcation
{
    Grade0 = 0,
    Grade1 = 1,
    Grade2 = 2,
    Grade3 = 3
}

public enum PeriodontalToothStatus
{
    Present = 0,
    Missing = 1,
    Implant = 2
}

public class PeriodontalExamSummary
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime ExaminedAt { get; set; }
    public string? Notes { get; set; }
    public string? RecordedByUserId { get; set; }
    public bool IsLatest { get; set; }
    public int RecordedSiteCount { get; set; }
    public decimal? BleedingPercent { get; set; }
    public decimal? PlaquePercent { get; set; }
    public decimal? MeanProbingDepthMm { get; set; }
    public int SitesDeepGe5 { get; set; }
    public int SitesDeepGe6 { get; set; }
}

public class PeriodontalExamDetail : PeriodontalExamSummary
{
    public List<PeriodontalToothModel> Teeth { get; set; } = [];
}

public class PeriodontalToothModel
{
    public int ToothNumber { get; set; }
    public PeriodontalToothStatus Status { get; set; }
    public PeriodontalMobility Mobility { get; set; }
    public PeriodontalFurcation Furcation { get; set; }
    public PeriodontalFurcation FacialFurcation { get; set; }
    public PeriodontalFurcation LingualFurcation { get; set; }
    public int? KeratinizedGingivaMm { get; set; }
    public string? Notes { get; set; }
    public List<PeriodontalSiteModel> Sites { get; set; } = [];
}

public class PeriodontalSiteModel
{
    public PeriodontalSurface Surface { get; set; }
    public PeriodontalSitePosition Position { get; set; }
    public int? ProbingDepthMm { get; set; }
    public int? RecessionMm { get; set; }
    public int? ClinicalAttachmentLevelMm { get; set; }
    public bool Bleeding { get; set; }
    public bool Plaque { get; set; }
    public bool Suppuration { get; set; }
}

public class CreatePeriodontalExamRequest
{
    public bool CopyLatest { get; set; } = true;
}

public class SavePeriodontalExamRequest
{
    public DateTime? ExaminedAt { get; set; }
    public string? Notes { get; set; }
    public List<PeriodontalToothModel> Teeth { get; set; } = [];
}

public class PeriodontalCompareResult
{
    public PeriodontalExamSummary Previous { get; set; } = new();
    public PeriodontalExamSummary Current { get; set; } = new();
    public decimal? BleedingPercentChange { get; set; }
    public int? SitesDeepGe5Change { get; set; }
    public List<PeriodontalToothCompare> Teeth { get; set; } = [];
}

public class PeriodontalToothCompare
{
    public int ToothNumber { get; set; }
    public int? PreviousMaxProbingDepth { get; set; }
    public int? CurrentMaxProbingDepth { get; set; }
    public int? ProbingDepthChange { get; set; }
    public int? PreviousMaxCal { get; set; }
    public int? CurrentMaxCal { get; set; }
    public int? CalChange { get; set; }
}

public sealed class PeriodontalLiveIndices
{
    public int RecordedSiteCount { get; init; }
    public decimal BleedingPercent { get; init; }
    public decimal PlaquePercent { get; init; }
    public decimal? MeanProbingDepthMm { get; init; }
    public int SitesDeepGe5 { get; init; }
    public int SitesDeepGe6 { get; init; }
}
