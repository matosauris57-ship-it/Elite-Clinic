namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Handlers
{
    public class ApproveTreatmentPlanCommandHandler : AppRequestHandler<ApproveTreatmentPlanCommand, TreatmentPlanDTO>
    {
        private readonly ITreatmentPlanService treatmentPlanService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public ApproveTreatmentPlanCommandHandler(
            ICurrentUserService currentUserService,
            ITreatmentPlanService treatmentPlanService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.treatmentPlanService = treatmentPlanService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<TreatmentPlanDTO>> Handle(ApproveTreatmentPlanCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var plan = await treatmentPlanService.ApproveAsync(request.PlanId, CurrentUserId, request.AcceptedByName, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                return Success(mapper.Map<TreatmentPlanDTO>(plan), "Presupuesto aceptado.");
            }
            catch (NotFoundException ex)
            {
                return NotFound<TreatmentPlanDTO>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<TreatmentPlanDTO>(ex.Message);
            }
        }
    }
}
