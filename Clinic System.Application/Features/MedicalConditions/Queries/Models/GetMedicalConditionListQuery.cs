namespace Clinic_System.Application.Features.MedicalConditions.Queries.Models
{
    public class GetMedicalConditionListQuery : IRequest<Response<List<MedicalConditionDTO>>>
    {
        public bool ActiveOnly { get; set; }
    }
}
