namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Handlers;

public class IssueTreatmentPlanCommandHandler : AppRequestHandler<IssueTreatmentPlanCommand, TreatmentPlanDTO>
{
    private readonly ITreatmentPlanService service;
    private readonly IMapper mapper;
    private readonly IUnitOfWork unitOfWork;

    public IssueTreatmentPlanCommandHandler(
        ICurrentUserService currentUserService, ITreatmentPlanService service, IMapper mapper, IUnitOfWork unitOfWork)
        : base(currentUserService)
    {
        this.service = service;
        this.mapper = mapper;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<TreatmentPlanDTO>> Handle(IssueTreatmentPlanCommand request, CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
            return Unauthorized<TreatmentPlanDTO>("Only doctors or admins can issue treatment plans.");

        try
        {
            var plan = await service.IssueAsync(request.PlanId, CurrentUserId, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<TreatmentPlanDTO>(plan), "Presupuesto entregado.");
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
