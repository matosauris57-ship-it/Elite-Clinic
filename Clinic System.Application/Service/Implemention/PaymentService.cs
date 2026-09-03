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

        public async Task<Payment> RefundAsync(int paymentId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var payment = await unitOfWork.PaymentsRepository.GetPaymentWithLinesAsync(paymentId, cancellationToken)
                ?? throw new NotFoundException($"Payment with ID {paymentId} not found.");

            payment.MarkAsRefunded(reason);
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
            var paidAmount = amount is > 0 ? Money.Normalize(amount.Value) : 0.01m;

            var payment = new Payment
            {
                AppointmentId = appointmentId,
                AmountPaid = paidAmount,
                PaymentStatus = PaymentStatus.Pending,
                PaymentDate = null
            };
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
