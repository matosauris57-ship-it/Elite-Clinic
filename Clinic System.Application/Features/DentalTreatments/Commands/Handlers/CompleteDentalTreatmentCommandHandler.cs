using Clinic_System.Application.DTOs.Inventory;

namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers
{
    public class CompleteDentalTreatmentCommandHandler : AppRequestHandler<CompleteDentalTreatmentCommand, DentalTreatmentDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IInventoryService inventoryService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IToothChartService toothChartService;

        public CompleteDentalTreatmentCommandHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IInventoryService inventoryService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IToothChartService toothChartService) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.inventoryService = inventoryService;
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

            if (!request.SkipMaterialConsumption)
            {
                var existingConsumption = await inventoryService.GetConsumptionByTreatmentAsync(treatment.Id, cancellationToken);
                if (existingConsumption == null)
                {
                    // Null lines => BOM por defecto; lista vacía => sin descuento.
                    var shouldConsume = request.MaterialConsumption != null
                        || treatment.TreatmentProcedureId.HasValue;

                    if (shouldConsume)
                    {
                        await inventoryService.ConfirmConsumptionAsync(
                            treatment.Id,
                            request.MaterialConsumption,
                            request.MaterialConsumptionNotes,
                            CurrentUserId,
                            request.AllowInsufficientStock,
                            cancellationToken);
                    }
                }
            }

            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<DentalTreatmentDTO>(treatment), "Treatment completed.");
        }
    }
}
