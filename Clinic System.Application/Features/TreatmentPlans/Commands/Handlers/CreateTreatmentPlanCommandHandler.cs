namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Handlers
{
    public class CreateTreatmentPlanCommandHandler : AppRequestHandler<CreateTreatmentPlanCommand, TreatmentPlanDTO>
    {
        private readonly ITreatmentPlanService treatmentPlanService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public CreateTreatmentPlanCommandHandler(
            ICurrentUserService currentUserService,
            ITreatmentPlanService treatmentPlanService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.treatmentPlanService = treatmentPlanService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<TreatmentPlanDTO>> Handle(CreateTreatmentPlanCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<TreatmentPlanDTO>("Only doctors or admins can create treatment plans.");

            var plan = await treatmentPlanService.CreateAsync(
                request.PatientId, request.Title, request.Notes, request.ValidUntil, request.DiscountAmount,
                request.Items, CurrentUserId, cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<TreatmentPlanDTO>(plan), "Treatment plan created.");
        }
    }
}
