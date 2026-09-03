namespace Clinic_System.Application.Features.Doctors.Queries.Handlers
{
    public class DoctorListQueryHandler : ResponseHandler, IRequestHandler<GetDoctorListQuery, Response<List<GetDoctorListDTO>>>
    {
        private readonly IDoctorService doctorService;
        private readonly IMapper mapper;
        private readonly ILogger<DoctorListQueryHandler> logger;

        public DoctorListQueryHandler(IDoctorService doctorService,
            IMapper mapper,
            ILogger<DoctorListQueryHandler> logger)
        {
            this.doctorService = doctorService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Response<List<GetDoctorListDTO>>> Handle(GetDoctorListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling GetDoctorListQuery (IncludeInactive={IncludeInactive})", request.IncludeInactive);

            var doctors = await doctorService.GetDoctorsListForAdminAsync(request.IncludeInactive, cancellationToken);
            var doctorsMapper = mapper.Map<List<GetDoctorListDTO>>(doctors.Where(d => d != null));

            logger.LogInformation("Successfully retrieved {Count} doctors for admin list", doctorsMapper.Count);

            return Success(doctorsMapper);
        }
    }
}
