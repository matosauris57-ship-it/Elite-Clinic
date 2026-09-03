namespace Clinic_System.Application.DTOs.Payment
{
    public class PaymentReceiptDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string AmountDisplay { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentMethodDisplay { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string PaidAt { get; set; } = string.Empty;
    }
}
