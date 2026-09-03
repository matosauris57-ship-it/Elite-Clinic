namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models;

public class RejectTreatmentPlanCommand : IRequest<Response<TreatmentPlanDTO>>
{
    [JsonIgnore]
    public int PlanId { get; set; }
    public string? Reason { get; set; }
}
