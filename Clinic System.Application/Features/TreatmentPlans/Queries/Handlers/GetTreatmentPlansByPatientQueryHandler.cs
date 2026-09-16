namespace Clinic_System.Application.Features.TreatmentPlans.Queries.Handlers
{
    public class GetTreatmentPlansByPatientQueryHandler : AppRequestHandler<GetTreatmentPlansByPatientQuery, List<TreatmentPlanDTO>>
    {
        private readonly ITreatmentPlanService treatmentPlanService;
        private readonly IMapper mapper;
        private readonly IClinicDataScopeService clinicScope;

        public GetTreatmentPlansByPatientQueryHandler(
            ICurrentUserService currentUserService,
            ITreatmentPlanService treatmentPlanService,
            IMapper mapper,
            IClinicDataScopeService clinicScope) : base(currentUserService)
        {
            this.treatmentPlanService = treatmentPlanService;
            this.mapper = mapper;
            this.clinicScope = clinicScope;
        }

        public override async Task<Response<List<TreatmentPlanDTO>>> Handle(GetTreatmentPlansByPatientQuery request, CancellationToken cancellationToken)
        {
            int patientId;
            if (CurrentDoctorId.HasValue)
            {
                if (request.PatientId <= 0)
                    return BadRequest<List<TreatmentPlanDTO>>("PatientId is required for doctors.");
                patientId = request.PatientId;
            }
            else
            {
                var (authorizedPatientId, error) = await GetAuthorizedPatientId(
                    request.PatientId,
                    AdminPermissionCatalog.Build("planes-tratamiento", AdminPermissionCatalog.Actions.View));
                if (error != null) return error;
                patientId = authorizedPatientId;
            }

            if (!await clinicScope.AllowsPatientAsync(patientId, cancellationToken))
                return Unauthorized<List<TreatmentPlanDTO>>("Solo puede consultar pacientes que haya atendido.");

            var plans = await treatmentPlanService.GetByPatientIdAsync(patientId, cancellationToken);
            return Success(mapper.Map<List<TreatmentPlanDTO>>(plans.ToList()));
        }
    }
}
