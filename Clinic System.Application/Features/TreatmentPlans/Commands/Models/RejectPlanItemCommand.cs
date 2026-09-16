namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Models
{
    public class RejectPlanItemCommand : IRequest<Response<TreatmentPlanDTO>>
    {
        public int ItemId { get; set; }
    }
}
