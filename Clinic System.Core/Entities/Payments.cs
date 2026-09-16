using Clinic_System.Core.Finance;

namespace Clinic_System.Core.Entities
{
    public class Payment : ISoftDelete, IAuditable
    {
        public virtual int Id { get; set; }
        public virtual decimal AmountPaid { get; set; }
        public virtual decimal DiscountAmount { get; set; }
        public virtual string? AdditionalNotes { get; set; }
        public virtual DateTime? PaymentDate { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public virtual int PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public virtual int? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        public virtual int? TreatmentPlanId { get; set; }
        public virtual TreatmentPlan? TreatmentPlan { get; set; }

        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
        public virtual ICollection<PaymentReceipt> Receipts { get; set; } = new List<PaymentReceipt>();

        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal Subtotal => InvoiceLines.Any()
            ? Money.Sum(InvoiceLines.Select(l => l.LineTotal))
            : Money.Normalize(AmountPaid) + Money.Normalize(DiscountAmount);

        public decimal InvoiceTotal => Money.MaxZero(Subtotal - Money.Normalize(DiscountAmount));

        public decimal AmountCollected
        {
            get
            {
                var paid = Money.Sum(Receipts.Where(r => r.IsActivePayment).Select(r => r.Amount));
                var refunded = Money.Sum(Receipts.Where(r => r.IsActiveRefund).Select(r => r.Amount));
                return Money.MaxZero(paid - refunded);
            }
        }

        public decimal Balance => Money.MaxZero(InvoiceTotal - AmountCollected);

        public bool CanEditInvoice =>
            (PaymentStatus is PaymentStatus.Pending or PaymentStatus.Failed)
            && AmountCollected <= 0;

        public bool CanAddInvoiceLines =>
            PaymentStatus is not PaymentStatus.Cancelled;

        public bool CanReceivePayment =>
            PaymentStatus is not PaymentStatus.Cancelled
            && Balance > 0;

        public bool CanRefund => AmountCollected > 0 && PaymentStatus is not PaymentStatus.Cancelled;

        public PaymentReceipt ApplyReceipt(decimal amount, PaymentMethod method, string? notes = null)
        {
            if (!CanReceivePayment)
                throw new InvalidOperationException("Esta factura no admite abonos.");

            amount = Money.Normalize(amount);
            if (amount <= 0)
                throw new InvalidOperationException("El abono debe ser mayor a cero.");

            if (amount > Balance)
                throw new InvalidOperationException($"El abono no puede superar el saldo ({Money.Format(Balance)}).");

            var receipt = AddReceipt(amount, method, PaymentReceiptKind.Payment, notes);
            RecalculateStatus();
            UpdatedAt = DateTime.Now;
            return receipt;
        }

        public PaymentReceipt ApplyRefund(decimal amount, PaymentMethod method, string? notes = null)
        {
            if (PaymentStatus == PaymentStatus.Cancelled)
                throw new InvalidOperationException("No se puede reembolsar una factura cancelada.");

            amount = Money.Normalize(amount);
            if (amount <= 0)
                throw new InvalidOperationException("El reembolso debe ser mayor a cero.");

            if (amount > AmountCollected)
                throw new InvalidOperationException($"El reembolso no puede superar lo cobrado ({Money.Format(AmountCollected)}).");

            var receipt = AddReceipt(amount, method, PaymentReceiptKind.Refund, notes);
            RecalculateStatus();
            UpdatedAt = DateTime.Now;
            return receipt;
        }

        public void VoidReceipt(int receiptId, string? reason = null)
        {
            if (PaymentStatus == PaymentStatus.Cancelled)
                throw new InvalidOperationException("No se pueden anular comprobantes de una factura cancelada.");

            var receipt = Receipts.FirstOrDefault(r => r.Id == receiptId)
                ?? throw new InvalidOperationException("No se encontró el comprobante.");

            receipt.Void(reason);
            RecalculateStatus();
            UpdatedAt = DateTime.Now;
        }

        public void ApplyDiscount(decimal discount)
        {
            if (PaymentStatus == PaymentStatus.Cancelled)
                throw new InvalidOperationException("No se puede aplicar descuento a una factura cancelada.");

            discount = Money.Normalize(discount);
            if (discount < 0)
                throw new InvalidOperationException("El descuento no puede ser negativo.");

            var subtotal = Subtotal;
            if (discount > subtotal)
                throw new InvalidOperationException("El descuento no puede superar el subtotal de la factura.");

            var net = Money.MaxZero(subtotal - discount);
            if (net < AmountCollected)
                throw new InvalidOperationException("El descuento dejaría el total por debajo de lo ya cobrado. Reembolse o anule abonos primero.");

            DiscountAmount = discount;
            RecalculateInvoiceAmount(subtotal);
        }

        public void RecalculateStatus()
        {
            if (PaymentStatus == PaymentStatus.Cancelled)
                return;

            SyncLastReceipt();

            var collected = AmountCollected;
            var hasRefunds = Receipts.Any(r => r.IsActiveRefund);
            if (collected <= 0)
                PaymentStatus = hasRefunds ? PaymentStatus.Refunded : PaymentStatus.Pending;
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
            if (AmountCollected <= 0 && PaymentStatus is not PaymentStatus.Paid and not PaymentStatus.PartiallyPaid)
                throw new InvalidOperationException("Only collected payments can be refunded.");

            if (AmountCollected > 0)
                ApplyRefund(AmountCollected, this.PaymentMethod ?? Clinic_System.Core.Enums.PaymentMethod.Cash, reason);
            else
                PaymentStatus = PaymentStatus.Refunded;

            AdditionalNotes = reason ?? AdditionalNotes;
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsCancelling(string? reason = null)
        {
            if (AmountCollected > 0)
                throw new InvalidOperationException("No se puede cancelar una factura con abonos. Reembolse o anule los abonos primero.");

            if (PaymentStatus != PaymentStatus.Pending && PaymentStatus != PaymentStatus.Failed && PaymentStatus != PaymentStatus.Refunded)
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
            {
                AmountPaid = Money.Normalize(amount.Value);
                RecalculateInvoiceAmount();
            }
            if (method.HasValue)
                PaymentMethod = method.Value;
            if (notes != null)
                AdditionalNotes = notes;
            UpdatedAt = DateTime.Now;
        }

        public void RecalculateInvoiceAmount(decimal? subtotal = null)
        {
            if (PaymentStatus == PaymentStatus.Cancelled)
                throw new InvalidOperationException("No se puede modificar una factura cancelada.");

            var sub = subtotal ?? Subtotal;
            if (DiscountAmount > sub)
                DiscountAmount = sub;

            var net = Money.MaxZero(sub - Money.Normalize(DiscountAmount));
            AmountPaid = net > 0 ? net : 0.01m;
            RecalculateStatus();
            UpdatedAt = DateTime.Now;
        }

        private PaymentReceipt AddReceipt(decimal amount, PaymentMethod method, PaymentReceiptKind kind, string? notes)
        {
            var receipt = new PaymentReceipt
            {
                Amount = amount,
                PaymentMethod = method,
                Kind = kind,
                Notes = notes,
                PaidAt = DateTime.Now
            };
            Receipts.Add(receipt);
            PaymentMethod = method;
            PaymentDate = receipt.PaidAt;
            return receipt;
        }

        private void SyncLastReceipt()
        {
            var last = Receipts
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.PaidAt)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

            if (last == null)
            {
                PaymentDate = AmountCollected > 0 ? PaymentDate : null;
                return;
            }

            PaymentMethod = last.PaymentMethod;
            PaymentDate = last.PaidAt;
        }
    }
}
