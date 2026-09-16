namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Handlers
{
    public class AcceptPlanItemCommandHandler : AppRequestHandler<AcceptPlanItemCommand, TreatmentPlanDTO>
    {
        private readonly ITreatmentPlanService treatmentPlanService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public AcceptPlanItemCommandHandler(
            ICurrentUserService currentUserService,
            ITreatmentPlanService treatmentPlanService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.treatmentPlanService = treatmentPlanService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<TreatmentPlanDTO>> Handle(AcceptPlanItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var plan = await treatmentPlanService.AcceptItemAsync(request.ItemId, CurrentUserId, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                plan = await treatmentPlanService.GetByIdAsync(plan.Id, cancellationToken) ?? plan;
                return Success(mapper.Map<TreatmentPlanDTO>(plan), "Procedimiento aceptado. Pasa al plan de tratamiento.");
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
