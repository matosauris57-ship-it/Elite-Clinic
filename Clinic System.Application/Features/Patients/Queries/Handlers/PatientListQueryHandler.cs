namespace Clinic_System.Application.Features.Patients.Queries.Handlers
{
    public class PatientListQueryHandler : ResponseHandler, IRequestHandler<GetPatientListQuery, Response<List<GetPatientListDTO>>>
    {
        private readonly IPatientService patientService;
        private readonly IMapper mapper;

        public PatientListQueryHandler(IPatientService patientService, IMapper mapper)
        {
            this.patientService = patientService;
            this.mapper = mapper;
        }

        public async Task<Response<List<GetPatientListDTO>>> Handle(GetPatientListQuery request, CancellationToken cancellationToken)
        {
            var patients = await patientService.GetPatientsListForAdminAsync(request.IncludeInactive, cancellationToken);
            var mapped = mapper.Map<List<GetPatientListDTO>>(patients.Where(p => p != null));
            return Success(mapped);
        }
    }
}
