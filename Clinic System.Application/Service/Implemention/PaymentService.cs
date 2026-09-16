namespace Clinic_System.Application.Service.Implemention
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<Payment> ConfirmPaymentAsync(int appointmentId, PaymentMethod method,
           string? notes = null, decimal? amount = null, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentByAppointmentIdAsync(appointmentId);

            if (payment == null || payment.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Refunded or PaymentStatus.Cancelled)
            {
                throw new NotFoundException($"No pending payment found for appointment ID {appointmentId}.");
            }

            payment.MarkAsPaid(method, notes, amount);

            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);

            return payment;
        }

        public async Task<Payment> CollectAsync(int paymentId, PaymentMethod method, string? notes = null, decimal? amount = null, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(paymentId, cancellationToken)
                ?? throw new NotFoundException($"Payment with ID {paymentId} not found.");

            if (payment.InvoiceTotal <= 0)
                throw new InvalidOperationException("El monto a cobrar debe ser mayor a cero. Agregue líneas o un monto válido.");

            var toCollect = amount ?? payment.Balance;
            payment.ApplyReceipt(toCollect, method, notes);
            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);
            return payment;
        }

        public async Task<Payment> RefundAsync(int paymentId, string? reason = null, decimal? amount = null, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(paymentId, cancellationToken)
                ?? throw new NotFoundException($"Payment with ID {paymentId} not found.");

            var toRefund = amount is > 0 ? Money.Normalize(amount.Value) : payment.AmountCollected;
            if (toRefund <= 0)
                throw new InvalidOperationException("No hay cobros para reembolsar.");

            payment.ApplyRefund(toRefund, payment.PaymentMethod ?? PaymentMethod.Cash, reason);
            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);
            return payment;
        }

        public async Task<Payment> ApplyDiscountAsync(int paymentId, decimal discountAmount, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(paymentId, cancellationToken)
                ?? throw new NotFoundException($"Payment with ID {paymentId} not found.");

            payment.ApplyDiscount(discountAmount);
            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);
            return payment;
        }

        public async Task<Payment> VoidReceiptAsync(int paymentId, int receiptId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(paymentId, cancellationToken)
                ?? throw new NotFoundException($"Payment with ID {paymentId} not found.");

            payment.VoidReceipt(receiptId, reason);
            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);
            return payment;
        }

        public async Task<Payment> CancelAsync(int paymentId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(paymentId, cancellationToken)
                ?? throw new NotFoundException($"Payment with ID {paymentId} not found.");

            payment.MarkAsCancelling(reason);
            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);
            return payment;
        }

        public async Task<Payment> CreatePaymentAsync(int appointmentId, decimal? amount = null, CancellationToken cancellationToken = default)
        {
            var existing = await unitOfWork.PaymentsRepository.GetPaymentByAppointmentIdAsync(appointmentId);
            if (existing != null)
                return existing;

            var appointment = await unitOfWork.AppointmentsRepository.GetAppointmentWithDetailsAsync(appointmentId, cancellationToken)
                ?? throw new NotFoundException($"Appointment with ID {appointmentId} not found.");

            PlanItem? item = appointment.PlanItem;
            if (item == null && appointment.PlanItemId is > 0)
            {
                item = await unitOfWork.TreatmentPlansRepository.GetItemWithPlanAsync(appointment.PlanItemId.Value, cancellationToken);
            }

            var billPlanItem = item is { CanInvoice: true };
            var quantity = billPlanItem ? Math.Max(1, item!.Quantity) : 1;
            var unitPrice = billPlanItem
                ? item!.UnitPrice
                : amount is > 0
                    ? Money.Normalize(amount.Value)
                    : appointment.QuotedAmount is > 0
                        ? Money.Normalize(appointment.QuotedAmount.Value)
                        : appointment.TreatmentProcedure is { Price: > 0 }
                            ? appointment.TreatmentProcedure.Price
                            : 0.01m;

            var description = item?.ProcedureName
                ?? appointment.TreatmentProcedure?.Name
                ?? "Consulta";
            if (description.Length > 300)
                description = description[..300];

            var note = $"Factura de la cita #{appointmentId}.";
            var payment = new Payment
            {
                AppointmentId = appointmentId,
                PatientId = appointment.PatientId,
                AmountPaid = Money.Multiply(unitPrice, quantity),
                PaymentStatus = PaymentStatus.Pending,
                AdditionalNotes = note,
                PaymentDate = null
            };

            payment.InvoiceLines.Add(new InvoiceLine
            {
                Description = description,
                ToothNumber = appointment.ToothNumber ?? item?.ToothNumber,
                Quantity = quantity,
                UnitPrice = unitPrice,
                DentalTreatmentId = item?.DentalTreatmentId
            });
            payment.RecalculateInvoiceAmount();

            if (billPlanItem)
            {
                var plan = item!.TreatmentPlan ?? appointment.TreatmentPlan;
                if (plan != null && plan.Status == TreatmentPlanStatus.Approved)
                {
                    item.InvoicedPayment = payment;
                    plan.AttachInvoice(payment);
                    unitOfWork.TreatmentPlansRepository.Update(plan, cancellationToken);
                }
            }

            appointment.Payment = payment;
            await unitOfWork.PaymentsRepository.AddAsync(payment, cancellationToken);
            return payment;
        }

        public async Task<Payment> FailedPaymentAsync(int appointmentId, CancellationToken cancellationToken = default 
            ,string? message = null)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentByAppointmentIdAsync(appointmentId);

            if (payment == null || payment.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Refunded or PaymentStatus.Cancelled)
            {
                throw new NotFoundException($"No pending payment found for appointment ID {appointmentId}.");
            }

            if (payment.AmountCollected > 0)
                throw new InvalidOperationException("No se puede marcar como fallida una factura con abonos.");

            payment.MarkAsFailed(message);

            unitOfWork.PaymentsRepository.Update(payment, cancellationToken);

            return payment;
        }
    }
}
