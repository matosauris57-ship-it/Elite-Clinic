namespace Clinic_System.Application.Features.Patients.Queries.Models
{
    public class GetPatientClinicalProfileQuery : IRequest<Response<PatientClinicalProfileDTO>>
    {
        public int PatientId { get; set; }
    }
}
