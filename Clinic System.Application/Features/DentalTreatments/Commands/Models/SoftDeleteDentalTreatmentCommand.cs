namespace Clinic_System.Application.Features.DentalTreatments.Commands.Models
{
    public class SoftDeleteDentalTreatmentCommand : IRequest<Response<string>>
    {
        public int TreatmentId { get; set; }
    }
}
