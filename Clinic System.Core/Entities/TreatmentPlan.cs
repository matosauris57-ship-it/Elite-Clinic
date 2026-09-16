using Clinic_System.Core.Finance;

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
        public virtual DateTime? IssuedAt { get; set; }
        public virtual DateTime? AcceptedAt { get; set; }
        public virtual string? AcceptedByName { get; set; }
        public virtual string? RejectionReason { get; set; }
        public virtual int? InvoicePaymentId { get; set; }
        public virtual Payment? InvoicePayment { get; set; }

        public virtual ICollection<PlanItem> Items { get; set; } = new List<PlanItem>();
        public virtual ICollection<Payment> Invoices { get; set; } = new List<Payment>();

        public virtual bool IsDeleted { get; set; } = false;
        public virtual DateTime? DeletedAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public decimal TotalAmount => Money.Sum(Items.Select(i => Money.Multiply(i.UnitPrice, i.Quantity)));

        public decimal FinalAmount => Money.MaxZero(TotalAmount - Money.Normalize(DiscountAmount));

        public IEnumerable<Payment> OpenInvoices =>
            Invoices.Where(p => p.PaymentStatus is not PaymentStatus.Cancelled and not PaymentStatus.Refunded);

        public decimal AmountBilled
        {
            get
            {
                if (Invoices.Count > 0)
                    return Money.Sum(OpenInvoices.Select(p => p.InvoiceTotal));
                if (InvoicePayment != null
                    && InvoicePayment.PaymentStatus is not PaymentStatus.Cancelled and not PaymentStatus.Refunded)
                    return InvoicePayment.InvoiceTotal;
                return 0;
            }
        }

        public decimal AmountCollectedOnAccount
        {
            get
            {
                if (Invoices.Count > 0)
                    return Money.Sum(Invoices.Where(p => p.PaymentStatus != PaymentStatus.Cancelled).Select(p => p.AmountCollected));
                return InvoicePayment is { PaymentStatus: not PaymentStatus.Cancelled }
                    ? InvoicePayment.AmountCollected
                    : 0;
            }
        }

        public decimal RemainingToBill => Money.MaxZero(FinalAmount - AmountBilled);

        public decimal RemainingToCollect => Money.MaxZero(FinalAmount - AmountCollectedOnAccount);

        public IEnumerable<PlanItem> BillableCompletedItems =>
            Items.Where(i => !i.IsDeleted && i.CanInvoice);

        public decimal UnbilledCompletedAmount =>
            Money.Sum(BillableCompletedItems.Select(i => i.LineTotal));

        public IReadOnlyList<int> InvoicePaymentIds
        {
            get
            {
                var ids = Invoices
                    .Where(p => p.PaymentStatus != PaymentStatus.Cancelled)
                    .Select(p => p.Id)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();
                if (ids.Count == 0 && InvoicePaymentId is > 0)
                    ids.Add(InvoicePaymentId.Value);
                return ids;
            }
        }

        public bool CanIssue => Status == TreatmentPlanStatus.Draft;
        public bool CanAccept => Status is TreatmentPlanStatus.Draft or TreatmentPlanStatus.Issued;
        public bool CanReject => Status is TreatmentPlanStatus.Draft or TreatmentPlanStatus.Issued;
        public bool CanInvoice => Status == TreatmentPlanStatus.Approved && RemainingToBill > 0 && !IsExpired;
        public bool IsExpired => ValidUntil.HasValue && ValidUntil.Value.Date < DateTime.Today;

        public void Issue()
        {
            if (!CanIssue)
                throw new InvalidOperationException("Solo un presupuesto en borrador se puede entregar.");

            Status = TreatmentPlanStatus.Issued;
            IssuedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public void Approve(string? acceptedByName = null)
        {
            if (!CanAccept)
                throw new InvalidOperationException("Solo un presupuesto en borrador o entregado se puede aceptar.");

            IssuedAt ??= DateTime.Now;
            Status = TreatmentPlanStatus.Approved;
            AcceptedAt = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(acceptedByName))
                AcceptedByName = acceptedByName.Trim();
            UpdatedAt = DateTime.Now;
        }

        public void Reject(string? reason = null)
        {
            if (!CanReject)
                throw new InvalidOperationException("Solo un presupuesto en borrador o entregado se puede rechazar.");

            Status = TreatmentPlanStatus.Rejected;
            if (!string.IsNullOrWhiteSpace(reason))
                RejectionReason = reason.Trim();
            UpdatedAt = DateTime.Now;
        }

        public void Complete()
        {
            if (Status != TreatmentPlanStatus.Approved)
                throw new InvalidOperationException("Only approved plans can be completed.");
            Status = TreatmentPlanStatus.Completed;
            UpdatedAt = DateTime.Now;
        }

        public void AttachInvoice(Payment payment)
        {
            if (Status != TreatmentPlanStatus.Approved)
                throw new InvalidOperationException("Acepte el presupuesto antes de facturarlo.");
            if (FinalAmount <= 0)
                throw new InvalidOperationException("El presupuesto no tiene un monto facturable.");

            payment.TreatmentPlan = this;
            payment.TreatmentPlanId = Id;
            Invoices.Add(payment);
            InvoicePayment = payment;
            UpdatedAt = DateTime.Now;
        }

        public IReadOnlyList<(string Description, int? ToothNumber, int Quantity, decimal UnitPrice)> ToBillableLines()
        {
            var items = Items.Where(i => !i.IsDeleted).ToList();
            if (items.Count == 0)
                return [];

            var subtotal = Money.Sum(items.Select(i => Money.Multiply(i.UnitPrice, i.Quantity)));
            var discount = Money.Normalize(DiscountAmount);
            if (discount > subtotal)
                discount = subtotal;

            var remainingDiscount = discount;
            var lines = new List<(string Description, int? ToothNumber, int Quantity, decimal UnitPrice)>(items.Count);

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];
                var lineTotal = Money.Multiply(item.UnitPrice, item.Quantity);
                var share = index == items.Count - 1 || subtotal <= 0
                    ? remainingDiscount
                    : Money.Normalize(discount * lineTotal / subtotal);
                if (share > remainingDiscount)
                    share = remainingDiscount;

                remainingDiscount = Money.MaxZero(remainingDiscount - share);
                var billed = Money.MaxZero(lineTotal - share);
                var unit = item.Quantity <= 0 ? 0 : Money.Normalize(billed / item.Quantity);
                lines.Add((item.ProcedureName, item.ToothNumber, item.Quantity, unit));
            }

            return lines;
        }
    }
}
