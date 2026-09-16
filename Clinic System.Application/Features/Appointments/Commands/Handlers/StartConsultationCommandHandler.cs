namespace Clinic_System.Application.Features.Appointments.Commands.Handlers
{
    public class StartConsultationCommandHandler : AppRequestHandler<StartConsultationCommand, AppointmentAgendaItemDTO>
    {
        private readonly IAppointmentService appointmentService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public StartConsultationCommandHandler(
            ICurrentUserService currentUserService,
            IAppointmentService appointmentService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.appointmentService = appointmentService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<AppointmentAgendaItemDTO>> Handle(StartConsultationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var appointment = await appointmentService.StartConsultationAsync(request.AppointmentId, cancellationToken);
                appointment = await unitOfWork.AppointmentsRepository.GetAppointmentWithDetailsAsync(appointment.Id, cancellationToken)
                    ?? appointment;
                return Success(mapper.Map<AppointmentAgendaItemDTO>(appointment), "Consulta iniciada.");
            }
            catch (NotFoundException ex)
            {
                return NotFound<AppointmentAgendaItemDTO>(ex.Message);
            }
            catch (InvalidAppointmentStateException ex)
            {
                return BadRequest<AppointmentAgendaItemDTO>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<AppointmentAgendaItemDTO>(ex.Message);
            }
        }
    }
}
