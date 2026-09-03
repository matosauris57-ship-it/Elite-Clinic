namespace Clinic_System.Application.DTOs.Dental
{
    public class TreatmentPlanDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Title { get; set; } = null!;
        public string? Notes { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime? ValidUntil { get; set; }
        public List<PlanItemDTO> Items { get; set; } = new();
    }
}
