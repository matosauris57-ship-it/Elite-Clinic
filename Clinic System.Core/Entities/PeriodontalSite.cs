namespace Clinic_System.Core.Entities;

public class PeriodontalSite
{
    public virtual int Id { get; set; }
    public virtual int PeriodontalToothId { get; set; }
    public virtual PeriodontalTooth Tooth { get; set; } = null!;
    public virtual PeriodontalSurface Surface { get; set; }
    public virtual PeriodontalSitePosition Position { get; set; }
    public virtual int? ProbingDepthMm { get; set; }
    public virtual int? RecessionMm { get; set; }
    public virtual int? ClinicalAttachmentLevelMm { get; set; }
    public virtual bool Bleeding { get; set; }
    public virtual bool Plaque { get; set; }
    public virtual bool Suppuration { get; set; }
}
