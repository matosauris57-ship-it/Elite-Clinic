namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models
{
    public class ApproveTreatmentPlanCommand : IRequest<Response<TreatmentPlanDTO>>
    {
        public int PlanId { get; set; }
    }
}
