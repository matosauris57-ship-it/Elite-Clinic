namespace Clinic_System.Application.Features.Doctors.Queries.Handlers
{
    public class DoctorListQueryHandler : ResponseHandler, IRequestHandler<GetDoctorListQuery, Response<List<GetDoctorListDTO>>>
    {
        private readonly IDoctorService doctorService;
        private readonly IMapper mapper;
        private readonly ILogger<DoctorListQueryHandler> logger;
        private readonly IClinicDataScopeService _clinicScope;

        public DoctorListQueryHandler(IDoctorService doctorService,
            IMapper mapper,
            ILogger<DoctorListQueryHandler> logger,
            IClinicDataScopeService clinicScope)
        {
            this.doctorService = doctorService;
            this.mapper = mapper;
            this.logger = logger;
            _clinicScope = clinicScope;
        }

        public async Task<Response<List<GetDoctorListDTO>>> Handle(GetDoctorListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling GetDoctorListQuery (IncludeInactive={IncludeInactive})", request.IncludeInactive);

            var doctors = await doctorService.GetDoctorsListForAdminAsync(request.IncludeInactive, cancellationToken);
            if (_clinicScope.RestrictsToOwnDoctorData && _clinicScope.ScopedDoctorId.HasValue)
                doctors = doctors.Where(d => d != null && d.Id == _clinicScope.ScopedDoctorId.Value).ToList();
            var doctorsMapper = mapper.Map<List<GetDoctorListDTO>>(doctors.Where(d => d != null));

            logger.LogInformation("Successfully retrieved {Count} doctors for admin list", doctorsMapper.Count);

            return Success(doctorsMapper);
        }
    }
}
