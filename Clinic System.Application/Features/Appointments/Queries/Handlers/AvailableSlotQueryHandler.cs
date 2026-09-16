namespace Clinic_System.Application.Features.Appointments.Queries.Handlers
{
    public class AvailableSlotQueryHandler : AppRequestHandler<GetAvailableSlotQuery, List<AvailableSlotDTO>>
    {
        private readonly IAppointmentService appointmentService;
        private readonly IMapper mapper;
        private readonly ILogger<AvailableSlotQueryHandler> logger;

        public AvailableSlotQueryHandler(
            ICurrentUserService currentUserService,
            IAppointmentService appointmentService,
            IMapper mapper,
            ILogger<AvailableSlotQueryHandler> logger) : base(currentUserService)
        {
            this.appointmentService = appointmentService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public override async Task<Response<List<AvailableSlotDTO>>> Handle(GetAvailableSlotQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var scopeError = await ValidateScopedDoctorSelection(request.DoctorId);
                if (scopeError != null)
                    return scopeError;

                logger.LogInformation("Fetching available slots for DoctorId={DoctorId} on Date={Date}", request.DoctorId, request.Date);

                var availableSlots = await appointmentService
                    .GetAvailableSlotsAsync(request.DoctorId, request.Date, cancellationToken);

                var availableSlotDTOs = mapper.Map<List<AvailableSlotDTO>>(availableSlots);

                logger.LogInformation("Retrieved {Count} available slots for DoctorId={DoctorId} on Date={Date}", availableSlotDTOs.Count, request.DoctorId, request.Date);

                return Success(availableSlotDTOs, "Available slots retrieved successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching available slots for DoctorId={DoctorId} on Date={Date}", request.DoctorId, request.Date);
                return BadRequest<List<AvailableSlotDTO>>("Error occurred while fetching available slots: " + ex.Message);
            }
        }
    }
}
