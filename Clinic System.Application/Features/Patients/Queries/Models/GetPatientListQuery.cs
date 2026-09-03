namespace Clinic_System.Application.Features.Patients.Queries.Models
{
    public class GetPatientListQuery : IRequest<Response<List<GetPatientListDTO>>>
    {
        public bool IncludeInactive { get; set; } = true;
    }
}
