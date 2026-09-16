namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models
{
    public class IssueTreatmentPlanCommand : IRequest<Response<TreatmentPlanDTO>>
    {
        public int PlanId { get; set; }
    }
}
