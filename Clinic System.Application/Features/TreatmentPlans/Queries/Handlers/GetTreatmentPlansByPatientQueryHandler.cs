namespace Clinic_System.Application.Features.TreatmentPlans.Queries.Handlers
{
    public class GetTreatmentPlansByPatientQueryHandler : AppRequestHandler<GetTreatmentPlansByPatientQuery, List<TreatmentPlanDTO>>
    {
        private readonly ITreatmentPlanService treatmentPlanService;
        private readonly IMapper mapper;

        public GetTreatmentPlansByPatientQueryHandler(
            ICurrentUserService currentUserService,
            ITreatmentPlanService treatmentPlanService,
            IMapper mapper) : base(currentUserService)
        {
            this.treatmentPlanService = treatmentPlanService;
            this.mapper = mapper;
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
                var (authorizedPatientId, error) = await GetAuthorizedPatientId(request.PatientId);
                if (error != null) return error;
                patientId = authorizedPatientId;
            }

            var plans = await treatmentPlanService.GetByPatientIdAsync(patientId, cancellationToken);
            return Success(mapper.Map<List<TreatmentPlanDTO>>(plans.ToList()));
        }
    }
}
