namespace Clinic_System.Application.Mapping.Appointments
{
    public partial class AppointmentProfile
    {
        public void GetAgendaForAdminMapping()
        {
            CreateMap<Appointment, AppointmentAgendaItemDTO>()
                .ForMember(d => d.AppointmentDate, o => o.MapFrom(s => s.AppointmentDate.ToString("yyyy-MM-dd HH:mm")))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.CancelledAt, o => o.MapFrom(s => s.CancelledAt))
                .ForMember(d => d.CancellationChannel, o => o.MapFrom(s => s.CancellationChannel.HasValue ? s.CancellationChannel.ToString() : null))
                .ForMember(d => d.CancellationComment, o => o.MapFrom(s => s.CancellationComment))
                .ForMember(d => d.PatientFullName, o => o.MapFrom(s => s.Patient.FullName))
                .ForMember(d => d.PatientPhone, o => o.MapFrom(s => s.Patient.Phone))
                .ForMember(d => d.PatientEmail, o => o.MapFrom(s => s.Patient.Email))
                .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.Doctor.FullName))
                .ForMember(d => d.DoctorPhone, o => o.MapFrom(s => s.Doctor.Phone))
                .ForMember(d => d.Specialization, o => o.MapFrom(s => s.Doctor.Specialization))
                .ForMember(d => d.PlanItemId, o => o.MapFrom(s => s.PlanItemId))
                .ForMember(d => d.TreatmentPlanId, o => o.MapFrom(s => s.TreatmentPlanId))
                .ForMember(d => d.PaymentId, o => o.MapFrom(s => s.Payment != null ? s.Payment.Id : (int?)null))
                .ForMember(d => d.ToothNumber, o => o.MapFrom(s => s.ToothNumber))
                .ForMember(d => d.ProcedureName, o => o.MapFrom(s =>
                    s.PlanItem != null
                        ? s.PlanItem.ProcedureName
                        : s.TreatmentProcedure != null ? s.TreatmentProcedure.Name : null));
        }
    }
}
