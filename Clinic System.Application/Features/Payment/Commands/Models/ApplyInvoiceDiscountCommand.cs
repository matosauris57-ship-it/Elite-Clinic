namespace Clinic_System.Application.Features.Payment.Commands.Models
{
    public class ApplyInvoiceDiscountCommand : IRequest<Response<PaymentDetailsDTO>>
    {
        [JsonIgnore]
        public int PaymentId { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? DiscountAmountInput { get; set; }
    }
}
