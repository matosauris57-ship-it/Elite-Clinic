namespace Clinic_System.Application.Features.Appointments.Commands.Handlers
{
    public class BookAppointmentCommandHandler : AppRequestHandler<BookAppointmentCommand, AppointmentDTO>
    {
        private readonly IAppointmentService appointmentService;
        private readonly IMapper mapper;
        private readonly IDoctorService doctorService;
        private readonly ICacheService cacheService;
        private readonly IUnitOfWork unitOfWork;
        private readonly INotificationsService notificationsService;
        private readonly ILogger<BookAppointmentCommandHandler> logger;
        public BookAppointmentCommandHandler(
            ICurrentUserService currentUserService,
            IAppointmentService appointmentService,
            IMapper mapper,
            IDoctorService doctorService,
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            ILogger<BookAppointmentCommandHandler> logger,
            INotificationsService notificationsService) : base(currentUserService)
        {
            this.appointmentService = appointmentService;
            this.mapper = mapper;
            this.doctorService = doctorService;
            this.notificationsService = notificationsService;
            this.cacheService = cacheService;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public override async Task<Response<AppointmentDTO>> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling BookAppointmentCommand for PatientId: {PatientId}, DoctorId: {DoctorId}", request.PatientId, request.DoctorId);

            var createPermission = AdminPermissionCatalog.Build(
                "agendar-cita",
                AdminPermissionCatalog.Actions.Create);
            var (authorizedId, errorResponse) = await GetAuthorizedPatientId(
                request.PatientId,
                createPermission);

            if (errorResponse != null) return errorResponse;

            request.PatientId = authorizedId;

            try
            {
                var newAppointment = await appointmentService.BookAppointmentAsync(
                               request.PatientId,
                               request.DoctorId,
                               request.AppointmentDate,
                               request.AppointmentTime,
                               cancellationToken,
                               request.TreatmentProcedureId,
                               request.QuotedAmount
                           );

                var appointmentDto = mapper.Map<AppointmentDTO>(newAppointment);
    
                logger.LogInformation("Appointment booked successfully for PatientId: {PatientId}, DoctorId: {DoctorId}", request.PatientId, request.DoctorId);

                await cacheService.RemoveByPrefixAsync(
                    $"UpcomingAppts_Patient_{newAppointment.PatientId}",  // تحديث مواعيد المريض
                    $"UpcomingAppts_Doctor_{newAppointment.DoctorId}",    // تحديث مواعيد الدكتور
                    $"DoctorApptsByStatus_{newAppointment.DoctorId}",     // تحديث قوائم حالات الدكتور
                    "AdminApptsByStatus",                                 // تحديث قوائم حالات الأدمن
                    "AdminStats"                                          // تحديث إحصائيات الداشبورد
                );

                string doctorIdentityUserId = await doctorService.GetDoctorUserIdAsync(newAppointment.DoctorId,cancellationToken);


                var notificationDto = new NotificationDTO
                {
                    Title = "New Appointment Booking",
                    Message = $"A new appointment has been booked for patient '{appointmentDto.PatientName}' on {newAppointment.AppointmentDate.ToString("dd/MM/yyyy at hh:mm tt")}",
                    NotificationType = "AppointmentCreated",
                    RelatedEntityId = newAppointment.Id 
                };

                if (!string.IsNullOrEmpty(doctorIdentityUserId))
                {
                    await notificationsService.SendToUserAsync(doctorIdentityUserId, notificationDto);
                }

                await notificationsService.SendToGroupAsync("Admins", notificationDto);

                return Created(appointmentDto, "Cita agendada correctamente.");
            }
            catch (SlotAlreadyBookedException ex)
            {
                logger.LogWarning("Booking failed: {ErrorMessage}", ex.Message);
                return BadRequest<AppointmentDTO>("El horario seleccionado ya no está disponible. Elija otro horario.");
            }
            catch (ValidationException ex)
            {
                logger.LogWarning("Booking validation failed: {ErrorMessage}", ex.Message);
                return BadRequest<AppointmentDTO>("No se puede agendar una cita en una fecha u hora pasada.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while booking appointment for PatientId: {PatientId}, DoctorId: {DoctorId}", request.PatientId, request.DoctorId);
                return BadRequest<AppointmentDTO>("No se pudo agendar la cita. Inténtelo nuevamente.");
            }
        }
    }
}