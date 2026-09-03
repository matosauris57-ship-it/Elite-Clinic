namespace Clinic_System.Application.Mapping.Appointments
{
    public partial class AppointmentProfile
    {
        public void GetAgendaForAdminMapping()
        {
            CreateMap<Appointment, AppointmentAgendaItemDTO>()
                .ForMember(d => d.AppointmentDate, o => o.MapFrom(s => s.AppointmentDate.ToString("yyyy-MM-dd HH:mm")))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.PatientFullName, o => o.MapFrom(s => s.Patient.FullName))
                .ForMember(d => d.PatientPhone, o => o.MapFrom(s => s.Patient.Phone))
                .ForMember(d => d.PatientEmail, o => o.MapFrom(s => s.Patient.Email))
                .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor.FullName))
                .ForMember(d => d.DoctorPhone, o => o.MapFrom(s => s.Doctor.Phone))
                .ForMember(d => d.Specialization, o => o.MapFrom(s => s.Doctor.Specialization));
        }
    }
}
