namespace Clinic_System.Application.Features.Doctors.Commands.Handlers
{
    public class RestoreDoctorCommandHandler : ResponseHandler, IRequestHandler<RestoreDoctorCommand, Response<Doctor>>
    {
        private readonly IDoctorService doctorService;
        private readonly IIdentityService identityService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICacheService cacheService;
        private readonly ILogger<RestoreDoctorCommandHandler> logger;

        public RestoreDoctorCommandHandler(
            IDoctorService doctorService,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<RestoreDoctorCommandHandler> logger)
        {
            this.doctorService = doctorService;
            this.identityService = identityService;
            this.unitOfWork = unitOfWork;
            this.cacheService = cacheService;
            this.logger = logger;
        }

        public async Task<Response<Doctor>> Handle(RestoreDoctorCommand request, CancellationToken cancellationToken)
        {
            var doctor = await doctorService.GetDoctorByIdIncludingDeletedAsync(request.Id, cancellationToken);

            if (doctor == null)
            {
                logger.LogWarning("Doctor with Id {DoctorId} not found", request.Id);
                return NotFound<Doctor>($"No se encontró el médico con Id {request.Id}");
            }

            if (!doctor.IsDeleted)
            {
                return BadRequest<Doctor>("El médico ya está activo");
            }

            var specialization = doctor.Specialization.Trim().ToLower();

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    logger.LogInformation("Restoring Doctor with Id {DoctorId}", request.Id);
                    await doctorService.RestoreDoctor(doctor, cancellationToken);

                    var result = await unitOfWork.SaveAsync();
                    if (result == 0)
                    {
                        logger.LogError("Failed to restore Doctor with Id {DoctorId}", request.Id);
                        return BadRequest<Doctor>("No se pudo reactivar el médico");
                    }

                    var restoredUser = await identityService.RestoreUserAsync(doctor.ApplicationUserId, cancellationToken);
                    if (!restoredUser)
                    {
                        logger.LogError("Failed to restore associated user for Doctor with Id {DoctorId}", request.Id);
                        return BadRequest<Doctor>("No se pudo reactivar la cuenta de acceso del médico");
                    }

                    transaction.Complete();

                    await cacheService.RemoveByPrefixAsync(
                        "DoctorsList",
                        $"DoctorListBySpecialization:{specialization}",
                        $"DoctorProfile_{request.Id}",
                        $"DoctorWithAppointmentsById:{request.Id}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while restoring Doctor with Id {DoctorId}", request.Id);
                    return BadRequest<Doctor>($"No se pudo reactivar el médico: {ex.Message}");
                }
            }

            logger.LogInformation("Doctor with Id {DoctorId} restored successfully", request.Id);
            return Deleted<Doctor>("Médico reactivado correctamente");
        }
    }
}
