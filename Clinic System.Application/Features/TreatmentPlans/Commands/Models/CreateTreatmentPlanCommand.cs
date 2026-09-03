namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models
{
    public class CreateTreatmentPlanCommand : IRequest<Response<TreatmentPlanDTO>>
    {
        public int PatientId { get; set; }
        public string Title { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<PlanItemInput> Items { get; set; } = new();
    }
}
