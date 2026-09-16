namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Handlers;

public class InvoiceTreatmentPlanCommandHandler : AppRequestHandler<InvoiceTreatmentPlanCommand, TreatmentPlanDTO>
{
    private readonly ITreatmentPlanService service;
    private readonly IMapper mapper;
    private readonly IUnitOfWork unitOfWork;

    public InvoiceTreatmentPlanCommandHandler(
        ICurrentUserService currentUserService, ITreatmentPlanService service, IMapper mapper, IUnitOfWork unitOfWork)
        : base(currentUserService)
    {
        this.service = service;
        this.mapper = mapper;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<TreatmentPlanDTO>> Handle(InvoiceTreatmentPlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await service.InvoiceAsync(
                request.PlanId, CurrentUserId, Money.Resolve(request.AmountInput, request.Amount), request.CompletedOnly, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            plan = await service.GetByIdAsync(plan.Id, cancellationToken) ?? plan;
            return Success(mapper.Map<TreatmentPlanDTO>(plan), "Presupuesto facturado.");
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
