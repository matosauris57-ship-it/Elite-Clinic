namespace Clinic_System.Application.Features.Payment.Commands.Models
{
    public class VoidPaymentReceiptCommand : IRequest<Response<PaymentDetailsDTO>>
    {
        [JsonIgnore]
        public int PaymentId { get; set; }
        [JsonIgnore]
        public int ReceiptId { get; set; }
        public string? Reason { get; set; }
    }
}
