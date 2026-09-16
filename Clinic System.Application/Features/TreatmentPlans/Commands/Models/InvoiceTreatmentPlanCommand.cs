namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models
{
    public class InvoiceTreatmentPlanCommand : IRequest<Response<TreatmentPlanDTO>>
    {
        public int PlanId { get; set; }
        public decimal? Amount { get; set; }
        public string? AmountInput { get; set; }
        public bool CompletedOnly { get; set; }
    }
}
