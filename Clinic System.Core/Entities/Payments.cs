using Clinic_System.Core.Finance;

namespace Clinic_System.Core.Entities
{
    public class Payment : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual decimal AmountPaid { get; set; }
        public virtual string? AdditionalNotes { get; set; }
        public virtual DateTime? PaymentDate { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public virtual int AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = null!;

        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
        public virtual ICollection<PaymentReceipt> Receipts { get; set; } = new List<PaymentReceipt>();

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal InvoiceTotal => InvoiceLines.Any()
            ? Money.Sum(InvoiceLines.Select(l => l.LineTotal))
            : Money.Normalize(AmountPaid);

        public decimal AmountCollected => Money.Sum(Receipts.Select(r => r.Amount));

        public decimal Balance => Money.MaxZero(InvoiceTotal - AmountCollected);

        public bool CanEditInvoice =>
            (PaymentStatus is PaymentStatus.Pending or PaymentStatus.Failed)
            && AmountCollected <= 0;

        public bool CanReceivePayment =>
            PaymentStatus is PaymentStatus.Pending or PaymentStatus.Failed or PaymentStatus.PartiallyPaid;

        public PaymentReceipt ApplyReceipt(decimal amount, PaymentMethod method, string? notes = null)
        {
            if (!CanReceivePayment)
                throw new InvalidOperationException("Esta factura no admite abonos.");

            amount = Money.Normalize(amount);
            if (amount <= 0)
                throw new InvalidOperationException("El abono debe ser mayor a cero.");

            if (amount > Balance)
                throw new InvalidOperationException($"El abono no puede superar el saldo ({Money.Format(Balance)}).");

            var receipt = new PaymentReceipt
            {
                Amount = amount,
                PaymentMethod = method,
                Notes = notes,
                PaidAt = DateTime.Now
            };
            Receipts.Add(receipt);

            PaymentMethod = method;
            PaymentDate = receipt.PaidAt;
            RecalculateStatus();
            UpdatedAt = DateTime.Now;
            return receipt;
        }

        public void RecalculateStatus()
        {
            if (PaymentStatus is PaymentStatus.Refunded or PaymentStatus.Cancelled)
                return;

            var collected = AmountCollected;
            if (collected <= 0)
                PaymentStatus = PaymentStatus.Pending;
            else if (collected < InvoiceTotal)
                PaymentStatus = PaymentStatus.PartiallyPaid;
            else
                PaymentStatus = PaymentStatus.Paid;
        }

        public void MarkAsPaid(PaymentMethod method, string? additionalNotes = null, decimal? amount = null)
        {
            var toCollect = amount ?? Balance;
            if (toCollect <= 0 && PaymentStatus == PaymentStatus.Paid)
                throw new InvalidOperationException("Payment already paid.");

            if (toCollect > 0)
                ApplyReceipt(toCollect, method, additionalNotes);
            else
            {
                PaymentStatus = PaymentStatus.Paid;
                PaymentMethod = method;
                AdditionalNotes = additionalNotes ?? AdditionalNotes;
                PaymentDate = DateTime.Now;
                UpdatedAt = DateTime.Now;
            }
        }

        public void MarkAsFailed(string? reason = null)
        {
            PaymentStatus = PaymentStatus.Failed;
            AdditionalNotes = reason;
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsRefunded(string? reason = null)
        {
            if (PaymentStatus != PaymentStatus.Paid && PaymentStatus != PaymentStatus.PartiallyPaid)
                throw new InvalidOperationException("Only collected payments can be refunded.");
            PaymentStatus = PaymentStatus.Refunded;
            AdditionalNotes = reason;
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsCancelling(string? reason = null)
        {
            if (AmountCollected > 0)
                throw new InvalidOperationException("No se puede cancelar una factura con abonos. Reembolse primero.");

            if (PaymentStatus != PaymentStatus.Pending && PaymentStatus != PaymentStatus.Failed)
                throw new InvalidOperationException("Only pending payments can be cancelled.");
            PaymentStatus = PaymentStatus.Cancelled;
            AdditionalNotes = reason;
            UpdatedAt = DateTime.Now;
        }

        public void UpdatePaymentDetails(decimal? amount = null, PaymentMethod? method = null, string? notes = null)
        {
            if (!CanEditInvoice)
            {
                throw new InvalidOperationException("Cannot update payment details for a Paid, Refunded or Cancelled payment.");
            }

            if (amount.HasValue)
                AmountPaid = Money.Normalize(amount.Value);
            if (method.HasValue)
                PaymentMethod = method.Value;
            if (notes != null)
                AdditionalNotes = notes;
            UpdatedAt = DateTime.Now;
        }
    }
}
