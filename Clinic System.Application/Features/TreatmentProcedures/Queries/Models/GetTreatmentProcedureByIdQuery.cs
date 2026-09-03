namespace Clinic_System.Application.Features.TreatmentProcedures.Queries.Models
{
    public class GetTreatmentProcedureByIdQuery : IRequest<Response<TreatmentProcedureDTO>>
    {
        public int Id { get; set; }
        public int? DoctorId { get; set; }
    }
}
