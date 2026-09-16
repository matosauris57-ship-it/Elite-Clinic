namespace Clinic_System.Application.Features.Appointments.Commands.Models
{
    public class StartConsultationCommand : IRequest<Response<AppointmentAgendaItemDTO>>
    {
        public int AppointmentId { get; set; }
    }
}
