namespace Clinic_System.Application.Mapping.Doctors
{
    public partial class DoctorProfile
    {
        public void UpdateDoctorMapping()
        {
            // من Command لـ Entity (عشان الحفظ)
            CreateMap<UpdateDoctorCommand, Doctor>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.ApplicationUserId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                     srcMember != null && (!(srcMember is string s) || !string.IsNullOrWhiteSpace(s))
                ));
            CreateMap<Doctor, UpdateDoctorDTO>();
        }
    }
}
