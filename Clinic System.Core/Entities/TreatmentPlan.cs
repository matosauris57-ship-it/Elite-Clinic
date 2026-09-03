namespace Clinic_System.Core.Entities
{
    public class TreatmentPlan : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;
        public virtual string Title { get; set; } = null!;
        public virtual string? Notes { get; set; }
        public virtual TreatmentPlanStatus Status { get; set; } = TreatmentPlanStatus.Draft;
        public virtual decimal DiscountAmount { get; set; }
        public virtual DateTime? ValidUntil { get; set; }

        public virtual ICollection<PlanItem> Items { get; set; } = new List<PlanItem>();

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);

        public decimal FinalAmount => Math.Max(0, TotalAmount - DiscountAmount);

        public void Approve()
        {
            if (Status != TreatmentPlanStatus.Draft)
                throw new InvalidOperationException("Only draft plans can be approved.");
            Status = TreatmentPlanStatus.Approved;
            UpdatedAt = DateTime.Now;
        }

        public void Reject(string? reason = null)
        {
            if (Status != TreatmentPlanStatus.Draft)
                throw new InvalidOperationException("Only draft plans can be rejected.");
            Status = TreatmentPlanStatus.Rejected;
            if (reason != null) Notes = reason;
            UpdatedAt = DateTime.Now;
        }

        public void Complete()
        {
            if (Status != TreatmentPlanStatus.Approved)
                throw new InvalidOperationException("Only approved plans can be completed.");
            Status = TreatmentPlanStatus.Completed;
            UpdatedAt = DateTime.Now;
        }
    }
}
