namespace Clinic_System.Application.Features.TreatmentPlans.Queries.Models
{
    public class GetTreatmentPlanByIdQuery : IRequest<Response<TreatmentPlanDTO>>
    {
        public int PlanId { get; set; }
    }
}
