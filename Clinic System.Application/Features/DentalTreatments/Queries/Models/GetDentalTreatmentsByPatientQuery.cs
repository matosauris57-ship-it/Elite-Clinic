namespace Clinic_System.Application.Features.DentalTreatments.Queries.Models
{
    public class GetDentalTreatmentsByPatientQuery : IRequest<Response<List<DentalTreatmentDTO>>>
    {
        public int PatientId { get; set; }
    }
}
