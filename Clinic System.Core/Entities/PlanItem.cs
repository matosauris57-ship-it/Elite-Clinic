namespace Clinic_System.Core.Entities
{
    public class PlanItem : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int TreatmentPlanId { get; set; }
        public virtual TreatmentPlan TreatmentPlan { get; set; } = null!;

        public virtual string ProcedureName { get; set; } = null!;
        public virtual int? TreatmentProcedureId { get; set; }
        public virtual TreatmentProcedure? TreatmentProcedure { get; set; }
        public virtual int? ToothNumber { get; set; }
        public virtual ToothSurface? ToothSurface { get; set; }
        public virtual int Quantity { get; set; } = 1;
        public virtual decimal UnitPrice { get; set; }
        public virtual string? Notes { get; set; }

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
