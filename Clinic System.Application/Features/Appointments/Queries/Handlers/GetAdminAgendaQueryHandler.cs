namespace Clinic_System.Application.Features.Appointments.Queries.Handlers
{
    public class GetAdminAgendaQueryHandler : AppRequestHandler<GetAdminAgendaQuery, PagedResult<AppointmentAgendaItemDTO>>
    {
        private readonly IAppointmentService appointmentService;
        private readonly IMapper mapper;

        public GetAdminAgendaQueryHandler(
            ICurrentUserService currentUserService,
            IAppointmentService appointmentService,
            IMapper mapper) : base(currentUserService)
        {
            this.appointmentService = appointmentService;
            this.mapper = mapper;
        }

        public override async Task<Response<PagedResult<AppointmentAgendaItemDTO>>> Handle(GetAdminAgendaQuery request, CancellationToken cancellationToken)
        {
            var appointments = await appointmentService.GetAgendaForAdminAsync(request, cancellationToken);
            var items = mapper.Map<List<AppointmentAgendaItemDTO>>(appointments.Items);

            return Success(new PagedResult<AppointmentAgendaItemDTO>(
                items,
                appointments.TotalCount,
                appointments.CurrentPage,
                appointments.PageSize));
        }
    }
}
