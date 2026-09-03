namespace Clinic_System.Application.Features.Payment.Commands.Models
{
    public class CancelPaymentCommand : IRequest<Response<PaymentDetailsDTO>>
    {
        [JsonIgnore]
        public int PaymentId { get; set; }
        public string? Reason { get; set; }
    }
}
