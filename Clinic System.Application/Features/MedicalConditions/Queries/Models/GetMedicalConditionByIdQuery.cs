namespace Clinic_System.Application.Features.MedicalConditions.Queries.Models
{
    public class GetMedicalConditionByIdQuery : IRequest<Response<MedicalConditionDTO>>
    {
        public int Id { get; set; }
    }
}
