namespace Clinic_System.Core.Entities
{
    public class PaymentReceipt : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int PaymentId { get; set; }
        public virtual Payment Payment { get; set; } = null!;

        public virtual decimal Amount { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        public virtual string? Notes { get; set; }
        public virtual DateTime PaidAt { get; set; }

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }
    }
}
