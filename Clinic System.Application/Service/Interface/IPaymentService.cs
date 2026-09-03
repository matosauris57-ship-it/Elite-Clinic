namespace Clinic_System.Application.Service.Interface
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(int appointmentId, decimal? amount = null, CancellationToken cancellationToken = default);
        Task<Payment> FailedPaymentAsync(int appointmentId, CancellationToken cancellationToken = default, string? message = null);
        Task<Payment> ConfirmPaymentAsync(int appointmentId, PaymentMethod method, string? notes = null, decimal? amount = null, CancellationToken cancellationToken = default);
        Task<Payment> CollectAsync(int paymentId, PaymentMethod method, string? notes = null, decimal? amount = null, CancellationToken cancellationToken = default);
        Task<Payment> RefundAsync(int paymentId, string? reason = null, CancellationToken cancellationToken = default);
        Task<Payment> CancelAsync(int paymentId, string? reason = null, CancellationToken cancellationToken = default);
    }
}
