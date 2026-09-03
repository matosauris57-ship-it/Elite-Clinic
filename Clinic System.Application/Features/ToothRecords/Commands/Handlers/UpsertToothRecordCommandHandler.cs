namespace Clinic_System.Application.Features.ToothRecords.Commands.Handlers
{
    public class UpsertToothRecordCommandHandler : AppRequestHandler<UpsertToothRecordCommand, ToothRecordDTO>
    {
        private readonly IToothRecordService toothRecordService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public UpsertToothRecordCommandHandler(
            ICurrentUserService currentUserService,
            IToothRecordService toothRecordService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.toothRecordService = toothRecordService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<ToothRecordDTO>> Handle(UpsertToothRecordCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<ToothRecordDTO>("Solo médicos o administradores pueden actualizar el odontograma.");

            await toothRecordService.BatchUpsertAsync(request.PatientId,
            [
                new OdontogramToothInput(request.ToothNumber, request.DiagnosisCondition, request.TreatmentCondition, request.Notes)
            ], cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);

            var records = await toothRecordService.GetByPatientIdAsync(request.PatientId, cancellationToken);
            var record = records.First(r => r.ToothNumber == request.ToothNumber);
            return Success(mapper.Map<ToothRecordDTO>(record), "Registro dental guardado correctamente.");
        }
    }
}
