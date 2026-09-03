namespace Clinic_System.Application.Features.DentalTreatments.Commands.Models;

public class StartDentalTreatmentCommand : IRequest<Response<DentalTreatmentDTO>>
{
    public int TreatmentId { get; set; }
}
