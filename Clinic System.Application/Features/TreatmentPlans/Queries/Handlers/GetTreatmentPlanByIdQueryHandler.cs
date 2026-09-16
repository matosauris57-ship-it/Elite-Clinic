namespace Clinic_System.Application.Features.TreatmentPlans.Queries.Handlers
{
    public class GetTreatmentPlanByIdQueryHandler : AppRequestHandler<GetTreatmentPlanByIdQuery, TreatmentPlanDTO>
    {
        private readonly ITreatmentPlanService treatmentPlanService;
        private readonly IMapper mapper;
        private readonly IClinicDataScopeService clinicScope;

        public GetTreatmentPlanByIdQueryHandler(
            ICurrentUserService currentUserService,
            ITreatmentPlanService treatmentPlanService,
            IMapper mapper,
            IClinicDataScopeService clinicScope) : base(currentUserService)
        {
            this.treatmentPlanService = treatmentPlanService;
            this.mapper = mapper;
            this.clinicScope = clinicScope;
        }

        public override async Task<Response<TreatmentPlanDTO>> Handle(GetTreatmentPlanByIdQuery request, CancellationToken cancellationToken)
        {
            var plan = await treatmentPlanService.GetByIdAsync(request.PlanId, cancellationToken);
            if (plan == null)
                return NotFound<TreatmentPlanDTO>("Presupuesto no encontrado.");

            if (CurrentDoctorId.HasValue && RestrictsToOwnDoctorData)
            {
                if (!await clinicScope.AllowsPatientAsync(plan.PatientId, cancellationToken))
                    return Unauthorized<TreatmentPlanDTO>("Solo puede consultar presupuestos de pacientes que haya atendido.");
                return Success(mapper.Map<TreatmentPlanDTO>(plan));
            }

            if (CurrentDoctorId.HasValue)
                return Success(mapper.Map<TreatmentPlanDTO>(plan));

            var (_, error) = await GetAuthorizedPatientId(
                plan.PatientId,
                AdminPermissionCatalog.Build("planes-tratamiento", AdminPermissionCatalog.Actions.View));
            if (error != null)
                return error;

            return Success(mapper.Map<TreatmentPlanDTO>(plan));
        }
    }
}
