namespace Clinic_System.Core.Entities
{
    public class PaymentReceipt : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual int PaymentId { get; set; }
        public virtual Payment Payment { get; set; } = null!;

        public virtual decimal Amount { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        public virtual PaymentReceiptKind Kind { get; set; } = PaymentReceiptKind.Payment;
        public virtual string? Notes { get; set; }
        public virtual DateTime PaidAt { get; set; }
        public virtual bool IsVoided { get; set; }
        public virtual DateTime? VoidedAt { get; set; }
        public virtual string? VoidReason { get; set; }

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public bool IsActive => !IsVoided && !IsDeleted;
        public bool IsActivePayment => IsActive && Kind == PaymentReceiptKind.Payment;
        public bool IsActiveRefund => IsActive && Kind == PaymentReceiptKind.Refund;

        public void Void(string? reason = null)
        {
            if (IsVoided)
                throw new InvalidOperationException("Este comprobante ya está anulado.");

            IsVoided = true;
            VoidedAt = DateTime.Now;
            VoidReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            UpdatedAt = DateTime.Now;
        }
    }
}
