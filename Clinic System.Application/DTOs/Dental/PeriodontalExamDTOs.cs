namespace Clinic_System.Application.DTOs.Dental;

public class PeriodontalExamSummaryDTO
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

public class PeriodontalExamDTO : PeriodontalExamSummaryDTO
{
    public List<PeriodontalToothDTO> Teeth { get; set; } = [];
}

public class PeriodontalToothDTO
{
    public int ToothNumber { get; set; }
    public PeriodontalToothStatus Status { get; set; }
    public PeriodontalMobility Mobility { get; set; }
    public PeriodontalFurcation Furcation { get; set; }
    public PeriodontalFurcation FacialFurcation { get; set; }
    public PeriodontalFurcation LingualFurcation { get; set; }
    public int? KeratinizedGingivaMm { get; set; }
    public string? Notes { get; set; }
    public List<PeriodontalSiteDTO> Sites { get; set; } = [];
}

public class PeriodontalSiteDTO
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

public class PeriodontalExamUpsertDTO
{
    public DateTime? ExaminedAt { get; set; }
    public string? Notes { get; set; }
    public List<PeriodontalToothDTO> Teeth { get; set; } = [];
}

public class PeriodontalCompareDTO
{
    public PeriodontalExamSummaryDTO Previous { get; set; } = null!;
    public PeriodontalExamSummaryDTO Current { get; set; } = null!;
    public decimal? BleedingPercentChange { get; set; }
    public int? SitesDeepGe5Change { get; set; }
    public List<PeriodontalToothCompareDTO> Teeth { get; set; } = [];
}

public class PeriodontalToothCompareDTO
{
    public int ToothNumber { get; set; }
    public int? PreviousMaxProbingDepth { get; set; }
    public int? CurrentMaxProbingDepth { get; set; }
    public int? ProbingDepthChange { get; set; }
    public int? PreviousMaxCal { get; set; }
    public int? CurrentMaxCal { get; set; }
    public int? CalChange { get; set; }
}
