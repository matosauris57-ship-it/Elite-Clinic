namespace Clinic_System.Application.Features.DentalHistory.Queries.Handlers
{
    public class GetDentalHistoryByPatientQueryHandler : AppRequestHandler<GetDentalHistoryByPatientQuery, DentalHistoryDTO>
    {
        private readonly IDentalHistoryService dentalHistoryService;
        private readonly IMapper mapper;
        private readonly IClinicDataScopeService clinicScope;

        public GetDentalHistoryByPatientQueryHandler(
            ICurrentUserService currentUserService,
            IDentalHistoryService dentalHistoryService,
            IMapper mapper,
            IClinicDataScopeService clinicScope) : base(currentUserService)
        {
            this.dentalHistoryService = dentalHistoryService;
            this.mapper = mapper;
            this.clinicScope = clinicScope;
        }

        public override async Task<Response<DentalHistoryDTO>> Handle(GetDentalHistoryByPatientQuery request, CancellationToken cancellationToken)
        {
            int patientId;
            if (CurrentDoctorId.HasValue)
            {
                if (request.PatientId <= 0)
                    return BadRequest<DentalHistoryDTO>("PatientId is required for doctors.");
                patientId = request.PatientId;
            }
            else
            {
                var (authorizedPatientId, error) = await GetAuthorizedPatientId(request.PatientId);
                if (error != null) return error;
                patientId = authorizedPatientId;
            }

            if (!await clinicScope.AllowsPatientAsync(patientId, cancellationToken))
                return Unauthorized<DentalHistoryDTO>("Solo puede consultar pacientes que haya atendido.");

            var history = await dentalHistoryService.GetByPatientIdAsync(patientId, cancellationToken);
            if (history == null)
                return NotFound<DentalHistoryDTO>("Dental history not found for this patient.");

            return Success(mapper.Map<DentalHistoryDTO>(history));
        }
    }
}
