namespace Clinic_System.Application.Features.DentalTreatments.Commands.Models
{
    public class CancelDentalTreatmentCommand : IRequest<Response<DentalTreatmentDTO>>
    {
        public int TreatmentId { get; set; }
        public string? Reason { get; set; }
    }
}
