namespace Clinic_System.Application.Features.Patients.Commands.Models
{
    public class RestorePatientCommand : IRequest<Response<Patient>>
    {
        public int Id { get; set; }
    }
}
