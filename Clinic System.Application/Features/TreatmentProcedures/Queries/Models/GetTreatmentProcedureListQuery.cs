namespace Clinic_System.Application.Features.TreatmentProcedures.Queries.Models
{
    public class GetTreatmentProcedureListQuery : IRequest<Response<List<TreatmentProcedureDTO>>>
    {
        public bool ActiveOnly { get; set; }
        public int? DoctorId { get; set; }
    }
}
