namespace Clinic_System.Application.Features.DentalTreatments.Queries.Models
{
    public class GetDentalTreatmentsByAppointmentQuery : IRequest<Response<List<DentalTreatmentDTO>>>
    {
        public int AppointmentId { get; set; }
    }
}
