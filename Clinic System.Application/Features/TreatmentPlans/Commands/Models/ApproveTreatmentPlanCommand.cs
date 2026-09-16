namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models
{
    public class ApproveTreatmentPlanCommand : IRequest<Response<TreatmentPlanDTO>>
    {
        [JsonIgnore]
        public int PlanId { get; set; }
        public string? AcceptedByName { get; set; }
    }
}
