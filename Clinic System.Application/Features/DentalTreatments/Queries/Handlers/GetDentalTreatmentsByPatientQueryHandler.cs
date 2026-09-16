namespace Clinic_System.Application.Features.DentalTreatments.Queries.Handlers
{
    public class GetDentalTreatmentsByPatientQueryHandler : AppRequestHandler<GetDentalTreatmentsByPatientQuery, List<DentalTreatmentDTO>>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;
        private readonly IClinicDataScopeService clinicScope;

        public GetDentalTreatmentsByPatientQueryHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper,
            IClinicDataScopeService clinicScope) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
            this.clinicScope = clinicScope;
        }

        public override async Task<Response<List<DentalTreatmentDTO>>> Handle(GetDentalTreatmentsByPatientQuery request, CancellationToken cancellationToken)
        {
            int patientId;
            if (CurrentDoctorId.HasValue)
            {
                if (request.PatientId <= 0)
                    return BadRequest<List<DentalTreatmentDTO>>("PatientId is required for doctors.");
                patientId = request.PatientId;
            }
            else
            {
                var (authorizedPatientId, error) = await GetAuthorizedPatientId(request.PatientId);
                if (error != null) return error;
                patientId = authorizedPatientId;
            }

            if (!await clinicScope.AllowsPatientAsync(patientId, cancellationToken))
                return Unauthorized<List<DentalTreatmentDTO>>("Solo puede consultar pacientes que haya atendido.");

            var treatments = await dentalTreatmentService.GetByPatientIdAsync(patientId, cancellationToken);
            return Success(mapper.Map<List<DentalTreatmentDTO>>(treatments.ToList()));
        }
    }
}
