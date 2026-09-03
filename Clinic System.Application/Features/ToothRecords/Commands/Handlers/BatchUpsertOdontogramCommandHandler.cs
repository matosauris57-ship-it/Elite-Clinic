namespace Clinic_System.Application.Features.ToothRecords.Commands.Handlers
{
    public class BatchUpsertOdontogramCommandHandler : AppRequestHandler<BatchUpsertOdontogramCommand, string>
    {
        private readonly IToothRecordService toothRecordService;
        private readonly IUnitOfWork unitOfWork;

        public BatchUpsertOdontogramCommandHandler(
            ICurrentUserService currentUserService,
            IToothRecordService toothRecordService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.toothRecordService = toothRecordService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<string>> Handle(BatchUpsertOdontogramCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            var isClinician = IsAdmin ||
                roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
                _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit));
            if (!isClinician)
                return Unauthorized<string>("Solo médicos o administradores autorizados pueden actualizar el odontograma.");

            var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit);
            var (patientId, accessError) = await GetAuthorizedPatientId(request.PatientId, permission);
            if (accessError != null && roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
                patientId = request.PatientId;
            else if (accessError != null)
                return accessError;

            var inputs = request.Teeth.Select(t =>
                new OdontogramToothInput(t.ToothNumber, t.DiagnosisCondition, t.TreatmentCondition, t.Notes));

            await toothRecordService.BatchUpsertAsync(patientId, inputs, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);

            return Success("Odontograma guardado correctamente", "Odontograma guardado correctamente");
        }
    }
}
