namespace Clinic_System.Application.Features.Payment.Commands.Models
{
    public class CollectPaymentCommand : IRequest<Response<PaymentDetailsDTO>>
    {
        [JsonIgnore]
        public int PaymentId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public decimal? Amount { get; set; }
        public string? AmountInput { get; set; }
    }
}
