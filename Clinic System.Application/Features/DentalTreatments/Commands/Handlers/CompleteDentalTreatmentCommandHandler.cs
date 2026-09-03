namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers
{
    public class CompleteDentalTreatmentCommandHandler : AppRequestHandler<CompleteDentalTreatmentCommand, DentalTreatmentDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IToothChartService toothChartService;

        public CompleteDentalTreatmentCommandHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IToothChartService toothChartService) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.toothChartService = toothChartService;
        }

        public override async Task<Response<DentalTreatmentDTO>> Handle(CompleteDentalTreatmentCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<DentalTreatmentDTO>("Only doctors or admins can complete treatments.");

            var treatment = await dentalTreatmentService.CompleteAsync(request.TreatmentId, CurrentUserId, cancellationToken);
            if (request.ClinicalResult != null && treatment.ToothNumber.HasValue)
            {
                await toothChartService.CreateEntryAsync(
                    treatment.PatientId,
                    treatment.ToothNumber.Value,
                    request.ClinicalResult.Surface,
                    ToothChartPhase.Completed,
                    request.ClinicalResult.Condition,
                    request.ClinicalResult.Severity,
                    request.ClinicalResult.Notes,
                    treatment.AppointmentId,
                    recordedByUserId: CurrentUserId,
                    cancellationToken: cancellationToken);
            }
            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<DentalTreatmentDTO>(treatment), "Treatment completed.");
        }
    }
}
