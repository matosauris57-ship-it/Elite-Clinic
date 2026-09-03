namespace Clinic_System.Application.Features.TreatmentPlans.Queries.Models
{
    public class GetTreatmentPlansByPatientQuery : IRequest<Response<List<TreatmentPlanDTO>>>
    {
        public int PatientId { get; set; }
    }
}
