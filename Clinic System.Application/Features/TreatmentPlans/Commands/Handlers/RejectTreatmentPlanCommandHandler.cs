namespace Clinic_System.Application.Features.TreatmentPlans.Commands.Handlers;

public class RejectTreatmentPlanCommandHandler : AppRequestHandler<RejectTreatmentPlanCommand, TreatmentPlanDTO>
{
    private readonly ITreatmentPlanService service;
    private readonly IMapper mapper;
    private readonly IUnitOfWork unitOfWork;

    public RejectTreatmentPlanCommandHandler(
        ICurrentUserService currentUserService, ITreatmentPlanService service, IMapper mapper, IUnitOfWork unitOfWork)
        : base(currentUserService)
    {
        this.service = service;
        this.mapper = mapper;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<TreatmentPlanDTO>> Handle(RejectTreatmentPlanCommand request, CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
            return Unauthorized<TreatmentPlanDTO>("Only doctors or admins can reject treatment plans.");

        var plan = await service.RejectAsync(request.PlanId, request.Reason, CurrentUserId, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        return Success(mapper.Map<TreatmentPlanDTO>(plan), "Treatment plan rejected.");
    }
}
