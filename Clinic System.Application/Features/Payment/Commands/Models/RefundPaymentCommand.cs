namespace Clinic_System.Application.Features.Payment.Commands.Models
{
    public class RefundPaymentCommand : IRequest<Response<PaymentDetailsDTO>>
    {
        [JsonIgnore]
        public int PaymentId { get; set; }
        public string? Reason { get; set; }
        public decimal? Amount { get; set; }
        public string? AmountInput { get; set; }
    }
}
