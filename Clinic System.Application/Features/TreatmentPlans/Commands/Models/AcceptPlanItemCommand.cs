namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models
{
    public class AcceptPlanItemCommand : IRequest<Response<TreatmentPlanDTO>>
    {
        public int ItemId { get; set; }
    }
}
