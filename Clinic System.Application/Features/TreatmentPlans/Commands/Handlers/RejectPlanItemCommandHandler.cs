namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Handlers
{
    public class RejectPlanItemCommandHandler : AppRequestHandler<RejectPlanItemCommand, TreatmentPlanDTO>
    {
        private readonly ITreatmentPlanService treatmentPlanService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public RejectPlanItemCommandHandler(
            ICurrentUserService currentUserService,
            ITreatmentPlanService treatmentPlanService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.treatmentPlanService = treatmentPlanService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<TreatmentPlanDTO>> Handle(RejectPlanItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var plan = await treatmentPlanService.RejectItemAsync(request.ItemId, CurrentUserId, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                plan = await treatmentPlanService.GetByIdAsync(plan.Id, cancellationToken) ?? plan;
                return Success(mapper.Map<TreatmentPlanDTO>(plan), "Procedimiento rechazado. No pasa al plan de tratamiento.");
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
