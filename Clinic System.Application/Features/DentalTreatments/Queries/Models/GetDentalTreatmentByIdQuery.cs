namespace Clinic_System.Application.Features.DentalTreatments.Queries.Models
{
    public class GetDentalTreatmentByIdQuery : IRequest<Response<DentalTreatmentListItemDTO>>
    {
        public int Id { get; set; }
    }
}
