namespace Clinic_System.Application.Features.Doctors.Commands.Handlers
{
    public class UpdateDoctorCommandHandler : AppRequestHandler<UpdateDoctorCommand, UpdateDoctorDTO>
    {
        private readonly IDoctorService doctorService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICacheService cacheService;
        private readonly ILogger<UpdateDoctorCommandHandler> logger;
        private readonly IIdentityService identityService;

        public UpdateDoctorCommandHandler(
            IDoctorService doctorService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<UpdateDoctorCommandHandler> logger,
            IIdentityService identityService) : base(currentUserService)
        {  
            this.doctorService = doctorService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.cacheService = cacheService;
            this.logger = logger;
            this.identityService = identityService;
        }

        public override async Task<Response<UpdateDoctorDTO>> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting update process for doctor profile with Id {DoctorId}.", request.Id);

            var authResult = await ValidateDoctorDirectoryAccess(request.Id, requireEdit: true);
            if (authResult != null)
                return authResult;

            var doctor = await doctorService.GetDoctorByIdAsync(request.Id);

            if (doctor == null)
            {
                logger.LogWarning("Doctor with Id {DoctorId} not found.", request.Id);
                return NotFound<UpdateDoctorDTO>($"Doctor with Id {request.Id} not found");
            }

            // 1. امسك التخصص القديم قبل التعديل
            var oldSpecialization = doctor.Specialization.Trim().ToLower();

            var wantsAccountUpdate =
                HasDoctorDirectoryPermission(AdminPermissionCatalog.Actions.Edit) &&
                !string.IsNullOrWhiteSpace(doctor.ApplicationUserId) &&
                (!string.IsNullOrWhiteSpace(request.UserName) ||
                 !string.IsNullOrWhiteSpace(request.Email) ||
                 !string.IsNullOrWhiteSpace(request.Password));

            if (wantsAccountUpdate)
            {
                var (accountOk, accountError) = await identityService.UpdateManagedUserAccountAsync(
                    doctor.ApplicationUserId,
                    request.UserName,
                    request.Email,
                    request.Password,
                    cancellationToken);

                if (!accountOk)
                    return BadRequest<UpdateDoctorDTO>(accountError ?? "No se pudo actualizar la cuenta de acceso.");
            }

            mapper.Map(request, doctor);
            if (request.ClearSignatureImage)
                doctor.SignatureImageUrl = null;

            await doctorService.UpdateDoctor(doctor, cancellationToken);

            var result = await unitOfWork.SaveAsync();

            if (result == 0)
            {
                logger.LogError("Failed to update doctor profile with Id {DoctorId} in the database.", request.Id);
                return BadRequest<UpdateDoctorDTO>("Failed to update doctor profile in the database.");
            }

            var doctorsMapper = mapper.Map<UpdateDoctorDTO>(doctor);
            if (!string.IsNullOrWhiteSpace(doctor.ApplicationUserId))
            {
                var (email, userName) = await identityService.GetUserEmailAndUserNameAsync(
                    doctor.ApplicationUserId, cancellationToken);
                doctorsMapper.Email = email;
                doctorsMapper.UserName = userName;
            }
            var newSpecialization = doctorsMapper.Specialization.Trim().ToLower();


            logger.LogInformation("Doctor profile with Id {DoctorId} updated successfully.", request.Id);

            await cacheService.RemoveByPrefixAsync(
                "DoctorsList",                                  // 1. بيمسح كل صفحات ليستة الدكاترة
                $"DoctorListBySpecialization:{oldSpecialization}", // 2. بيمسح كل صفحات التخصصات القديمة
                $"DoctorListBySpecialization:{newSpecialization}", // 2. بيمسح كل صفحات التخصصات الجديدة
                $"DoctorProfile_{request.Id}",                  // 3. بيمسح البروفايل القديم بتاع الدكتور ده
                $"DoctorWithAppointmentsById:{request.Id}"      // 4. بيمسح مواعيد الدكتور ده
            );

            return Success<UpdateDoctorDTO>(doctorsMapper, "Doctor updated successfully");
        }
    }
}