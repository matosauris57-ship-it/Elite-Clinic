namespace Clinic_System.Application.DTOs.Dental
{
    public class TreatmentPlanDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientPhone { get; set; }
        public string? PatientNationalId { get; set; }
        public string Title { get; set; } = null!;
        public string? Notes { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string TotalAmountDisplay { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public string DiscountAmountDisplay { get; set; } = string.Empty;
        public decimal FinalAmount { get; set; }
        public string FinalAmountDisplay { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public string? AcceptedByName { get; set; }
        public string? RejectionReason { get; set; }
        public int? InvoicePaymentId { get; set; }
        public List<int> InvoicePaymentIds { get; set; } = [];
        public bool CanInvoice { get; set; }
        public decimal AmountBilled { get; set; }
        public string AmountBilledDisplay { get; set; } = string.Empty;
        public decimal AmountCollectedOnAccount { get; set; }
        public string AmountCollectedOnAccountDisplay { get; set; } = string.Empty;
        public decimal RemainingToBill { get; set; }
        public string RemainingToBillDisplay { get; set; } = string.Empty;
        public decimal RemainingToCollect { get; set; }
        public string RemainingToCollectDisplay { get; set; } = string.Empty;
        public decimal UnbilledCompletedAmount { get; set; }
        public string UnbilledCompletedAmountDisplay { get; set; } = string.Empty;
        public List<PlanItemDTO> Items { get; set; } = new();
    }
}
