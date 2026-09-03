namespace Clinic_System.Core.Entities;

public class PeriodontalTooth
{
    public virtual int Id { get; set; }
    public virtual int PeriodontalExamId { get; set; }
    public virtual PeriodontalExam Exam { get; set; } = null!;
    public virtual int ToothNumber { get; set; }
    public virtual PeriodontalToothStatus Status { get; set; }
    public virtual PeriodontalMobility Mobility { get; set; }
    public virtual PeriodontalFurcation FacialFurcation { get; set; }
    public virtual PeriodontalFurcation LingualFurcation { get; set; }
    public virtual int? KeratinizedGingivaMm { get; set; }
    public virtual string? Notes { get; set; }
    public virtual ICollection<PeriodontalSite> Sites { get; set; } = new List<PeriodontalSite>();
}
