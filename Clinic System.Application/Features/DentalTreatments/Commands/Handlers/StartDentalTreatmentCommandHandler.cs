namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers;

public class StartDentalTreatmentCommandHandler : AppRequestHandler<StartDentalTreatmentCommand, DentalTreatmentDTO>
{
    private readonly IDentalTreatmentService service;
    private readonly IMapper mapper;
    private readonly IUnitOfWork unitOfWork;

    public StartDentalTreatmentCommandHandler(
        ICurrentUserService currentUserService, IDentalTreatmentService service, IMapper mapper, IUnitOfWork unitOfWork)
        : base(currentUserService)
    {
        this.service = service;
        this.mapper = mapper;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<DentalTreatmentDTO>> Handle(StartDentalTreatmentCommand request, CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
            return Unauthorized<DentalTreatmentDTO>("Only doctors or admins can start treatments.");

        var treatment = await service.StartAsync(request.TreatmentId, CurrentUserId, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        return Success(mapper.Map<DentalTreatmentDTO>(treatment), "Treatment started.");
    }
}
