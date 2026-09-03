namespace Clinic_System.Core.Entities;

public class PeriodontalExam : ISoftDelete, IAuditable
{
    public virtual int Id { get; set; }
    public virtual int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;
    public virtual int? DoctorId { get; set; }
    public virtual Doctor? Doctor { get; set; }
    public virtual DateTime ExaminedAt { get; set; }
    public virtual string? Notes { get; set; }
    public virtual int RecordedSiteCount { get; set; }
    public virtual decimal? BleedingPercent { get; set; }
    public virtual decimal? PlaquePercent { get; set; }
    public virtual decimal? MeanProbingDepthMm { get; set; }
    public virtual int SitesDeepGe5 { get; set; }
    public virtual int SitesDeepGe6 { get; set; }
    public virtual string? RecordedByUserId { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime? DeletedAt { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual ICollection<PeriodontalTooth> Teeth { get; set; } = new List<PeriodontalTooth>();
}
