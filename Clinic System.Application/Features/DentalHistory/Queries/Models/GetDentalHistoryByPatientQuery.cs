namespace Clinic_System.Application.Features.DentalHistory.Queries.Models
{
    public class GetDentalHistoryByPatientQuery : IRequest<Response<DentalHistoryDTO>>
    {
        public int PatientId { get; set; }
    }
}
