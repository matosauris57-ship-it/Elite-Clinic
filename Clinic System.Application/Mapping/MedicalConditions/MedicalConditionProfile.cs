namespace Clinic_System.Application.Mapping.MedicalConditions
{
    public class MedicalConditionProfile : Profile
    {
        public MedicalConditionProfile()
        {
            CreateMap<MedicalCondition, MedicalConditionDTO>();
        }
    }
}
