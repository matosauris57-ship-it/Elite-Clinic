namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers
{
    public class CancelDentalTreatmentCommandHandler : AppRequestHandler<CancelDentalTreatmentCommand, DentalTreatmentDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public CancelDentalTreatmentCommandHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<DentalTreatmentDTO>> Handle(CancelDentalTreatmentCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<DentalTreatmentDTO>("Only doctors or admins can cancel treatments.");

            var treatment = await dentalTreatmentService.CancelAsync(request.TreatmentId, request.Reason, CurrentUserId, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<DentalTreatmentDTO>(treatment), "Dental treatment cancelled.");
        }
    }
}
