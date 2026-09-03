namespace Clinic_System.Application.Features.Doctors.Commands.Handlers
{
    public class SoftDeleteDoctorCommandHandler : ResponseHandler, IRequestHandler<SoftDeleteDoctorCommand, Response<Doctor>>
    {
        private readonly IDoctorService doctorService;
        private readonly IIdentityService identityService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICacheService cacheService;
        private readonly ILogger<SoftDeleteDoctorCommandHandler> logger;

        public SoftDeleteDoctorCommandHandler(IDoctorService doctorService
            , IIdentityService identityService, IUnitOfWork unitOfWork, ICacheService cacheService, ILogger<SoftDeleteDoctorCommandHandler> logger)
        {
            this.doctorService = doctorService;
            this.identityService = identityService;
            this.unitOfWork = unitOfWork;
            this.cacheService = cacheService;
            this.logger = logger;
        }

        public async Task<Response<Doctor>> Handle(SoftDeleteDoctorCommand request, CancellationToken cancellationToken)
        {
            var doctor = await doctorService.GetDoctorByIdIncludingDeletedAsync(request.Id, cancellationToken);

            if (doctor == null)
            {
                logger.LogWarning("Doctor with Id {DoctorId} not found", request.Id);
                return NotFound<Doctor>($"No se encontró el médico con Id {request.Id}");
            }

            if (doctor.IsDeleted)
            {
                return BadRequest<Doctor>("El médico ya está deshabilitado");
            }

            var specialization = doctor.Specialization.Trim().ToLower();

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    logger.LogInformation("Disabling Doctor with Id {DoctorId}", request.Id);
                    await doctorService.SoftDeleteDoctor(doctor, cancellationToken);

                    var result = await unitOfWork.SaveAsync();
                    if (result == 0)
                    {
                        logger.LogError("Failed to disable Doctor with Id {DoctorId}", request.Id);
                        return BadRequest<Doctor>("No se pudo deshabilitar el médico");
                    }

                    var isDeletedUser = await identityService.SoftDeleteUserAsync(doctor.ApplicationUserId, cancellationToken);

                    if (!isDeletedUser)
                    {
                        logger.LogError("Failed to disable associated user for Doctor with Id {DoctorId}", request.Id);
                        return BadRequest<Doctor>("No se pudo deshabilitar la cuenta de acceso del médico");
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
                    logger.LogError(ex, "An error occurred while disabling Doctor with Id {DoctorId}", request.Id);
                    return BadRequest<Doctor>($"No se pudo deshabilitar el médico: {ex.Message}");
                }
            }

            logger.LogInformation("Doctor with Id {DoctorId} disabled successfully", request.Id);
            return Deleted<Doctor>("Médico deshabilitado correctamente");
        }
    }
}
