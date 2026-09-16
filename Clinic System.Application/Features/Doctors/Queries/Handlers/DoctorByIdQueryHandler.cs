namespace Clinic_System.Application.Features.Doctors.Queries.Handlers
{
    public class DoctorByIdQueryHandler : AppRequestHandler<GetDoctorByIdQuery, GetDoctorDTO>
    {
        private readonly IDoctorService doctorService;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;
        private readonly ILogger<DoctorByIdQueryHandler> logger;
        private readonly IIdentityService identityService;

        public DoctorByIdQueryHandler(ICurrentUserService currentUserService,
            IDoctorService doctorService,
            IMapper mapper,
            ILogger<DoctorByIdQueryHandler> logger,
            ICacheService cacheService,
            IIdentityService identityService) : base(currentUserService)
        {
            this.doctorService = doctorService;
            this.mapper = mapper;
            this.logger = logger;
            this.cacheService = cacheService;
            this.identityService = identityService;
        }

        public override async Task<Response<GetDoctorDTO>> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling GetDoctorByIdQuery for ID: {Id}", request.Id);


            var authResult = await ValidateDoctorDirectoryAccess(request.Id, requireEdit: false);
            if (authResult != null)
                return authResult;

            // أ. بناء مفتاح مميز للصفحة دي تحديداً
            string cacheKey = $"DoctorProfile_{request.Id}";

            // ب. نسأل الـ Redis: "هل عندك الداتا دي؟"
            var cachedDoctor = await cacheService.GetDataAsync<GetDoctorDTO>(cacheKey);

            // ج. لو الداتا موجودة في الكاش، هنرجعها فوراً ومش هنكمل باقي الكود (وفرنا رحلة للداتابيز)
            if (cachedDoctor != null)
            {
                if (string.IsNullOrWhiteSpace(cachedDoctor.Email) &&
                    !string.IsNullOrWhiteSpace(cachedDoctor.ApplicationUserId))
                {
                    var (email, userName) = await identityService.GetUserEmailAndUserNameAsync(
                        cachedDoctor.ApplicationUserId, cancellationToken);
                    cachedDoctor.Email = email;
                    cachedDoctor.UserName = userName;
                }

                logger.LogInformation("Successfully retrieved doctor from CACHE for {CacheKey}", cacheKey);
                return Success(cachedDoctor);
            }

            var doctor = await doctorService.GetDoctorByIdAsync(request.Id, cancellationToken);

            if (doctor == null)
            {
                logger.LogWarning("Doctor with ID: {Id} not found.", request.Id);
                return NotFound<GetDoctorDTO>();
            }

            var doctorsMapper = mapper.Map<GetDoctorDTO>(doctor);
            if (!string.IsNullOrWhiteSpace(doctor.ApplicationUserId))
            {
                var (email, userName) = await identityService.GetUserEmailAndUserNameAsync(
                    doctor.ApplicationUserId, cancellationToken);
                doctorsMapper.Email = email;
                doctorsMapper.UserName = userName;
            }

            logger.LogInformation("Successfully retrieved doctor with ID: {Id}", request.Id);

            await cacheService.SetDataAsync(cacheKey, doctorsMapper, TimeSpan.FromMinutes(30));

            return Success(doctorsMapper);
        }
    }
}
