namespace Clinic_System.Application.Features.DentalHistory.Commands.Handlers
{
    public class UpsertDentalHistoryCommandHandler : AppRequestHandler<UpsertDentalHistoryCommand, DentalHistoryDTO>
    {
        private readonly IDentalHistoryService dentalHistoryService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public UpsertDentalHistoryCommandHandler(
            ICurrentUserService currentUserService,
            IDentalHistoryService dentalHistoryService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.dentalHistoryService = dentalHistoryService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<DentalHistoryDTO>> Handle(UpsertDentalHistoryCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (roles.Contains("Patient") && !roles.Contains("Admin") && !roles.Contains("Doctor"))
            {
                var authResult = await ValidatePatientAccess(request.PatientId);
                if (authResult != null) return authResult;
            }

            var history = await dentalHistoryService.CreateOrUpdateAsync(
                request.PatientId,
                request.Allergies,
                request.CurrentMedication,
                request.SystemicDiseases,
                request.PreviousDentalTreatments,
                request.BloodPressure,
                request.OtherDiseases,
                request.ReasonForConsultation,
                request.Diagnosis,
                request.ClinicalObservations,
                request.HasBleedingGums,
                request.HasSensitiveTeeth,
                request.HasBruxism,
                request.IsSmoker,
                request.AdditionalNotes,
                request.SelectedConditionIds,
                cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);

            var dto = mapper.Map<DentalHistoryDTO>(history);
            var conditions = await unitOfWork.PatientMedicalConditionsRepository.GetByPatientIdAsync(request.PatientId, cancellationToken);
            dto.SelectedConditionIds = conditions.Select(c => c.MedicalConditionId).ToList();

            return Success(dto, "Antecedentes guardados correctamente.");
        }
    }
}
