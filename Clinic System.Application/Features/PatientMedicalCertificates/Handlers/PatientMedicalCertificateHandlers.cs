namespace Clinic_System.Application.Features.PatientMedicalCertificates.Handlers;

public class ListMedicalCertificateOptionsQueryHandler : AppRequestHandler<ListMedicalCertificateOptionsQuery, MedicalCertificateOptionsDTO>
{
    private readonly IPatientMedicalCertificateService service;

    public ListMedicalCertificateOptionsQueryHandler(
        ICurrentUserService currentUserService,
        IPatientMedicalCertificateService service) : base(currentUserService)
    {
        this.service = service;
    }

    public override Task<Response<MedicalCertificateOptionsDTO>> Handle(
        ListMedicalCertificateOptionsQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(Success(new MedicalCertificateOptionsDTO
        {
            CertificateTypes = service.ListCertificateTypes().ToList(),
            Purposes = service.ListPurposes().ToList(),
            DiagnosisSuggestions = service.ListDiagnosisSuggestions().ToList()
        }));
}

public class ListPatientMedicalCertificatesQueryHandler : AppRequestHandler<ListPatientMedicalCertificatesQuery, List<PatientMedicalCertificateSummaryDTO>>
{
    private readonly IPatientMedicalCertificateService service;

    public ListPatientMedicalCertificatesQueryHandler(
        ICurrentUserService currentUserService,
        IPatientMedicalCertificateService service) : base(currentUserService)
    {
        this.service = service;
    }

    public override async Task<Response<List<PatientMedicalCertificateSummaryDTO>>> Handle(
        ListPatientMedicalCertificatesQuery request,
        CancellationToken cancellationToken)
    {
        var error = await AuthorizeView(request.PatientId);
        if (error != null)
            return error;

        var certificates = await service.ListByPatientAsync(request.PatientId, cancellationToken);
        return Success(certificates.Select(PatientMedicalCertificateService.MapSummary).ToList());
    }

    private async Task<Response<List<PatientMedicalCertificateSummaryDTO>>?> AuthorizeView(int patientId)
    {
        if (await CanViewAsync())
            return null;
        var (_, error) = await GetAuthorizedPatientId(patientId, AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.View));
        return error;
    }

    private async Task<bool> CanViewAsync()
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        return IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.View))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.View));
    }
}

public class GetPatientMedicalCertificateQueryHandler : AppRequestHandler<GetPatientMedicalCertificateQuery, PatientMedicalCertificateDTO>
{
    private readonly IPatientMedicalCertificateService service;

    public GetPatientMedicalCertificateQueryHandler(
        ICurrentUserService currentUserService,
        IPatientMedicalCertificateService service) : base(currentUserService)
    {
        this.service = service;
    }

    public override async Task<Response<PatientMedicalCertificateDTO>> Handle(
        GetPatientMedicalCertificateQuery request,
        CancellationToken cancellationToken)
    {
        var certificate = await service.GetAsync(request.CertificateId, cancellationToken);
        var error = await AuthorizeView(certificate.PatientId);
        if (error != null)
            return error;

        return Success(PatientMedicalCertificateService.MapDetail(certificate));
    }

    private async Task<Response<PatientMedicalCertificateDTO>?> AuthorizeView(int patientId)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var canView = IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.View))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.View));
        if (canView)
            return null;
        var (_, error) = await GetAuthorizedPatientId(patientId, AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.View));
        return error;
    }
}

public class CreatePatientMedicalCertificateCommandHandler : AppRequestHandler<CreatePatientMedicalCertificateCommand, PatientMedicalCertificateDTO>
{
    private readonly IPatientMedicalCertificateService service;
    private readonly IUnitOfWork unitOfWork;

    public CreatePatientMedicalCertificateCommandHandler(
        ICurrentUserService currentUserService,
        IPatientMedicalCertificateService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<PatientMedicalCertificateDTO>> Handle(
        CreatePatientMedicalCertificateCommand request,
        CancellationToken cancellationToken)
    {
        if (!await CanEditAsync())
            return Unauthorized<PatientMedicalCertificateDTO>("Solo personal clínico autorizado puede emitir certificados.");

        var doctorError = ValidateDoctorAssignment(request.DoctorId, null);
        if (doctorError != null)
            return doctorError;

        var created = await service.CreateAsync(
            request.PatientId,
            ToUpsert(request, CurrentDoctorId),
            CurrentDoctorId,
            CurrentUserId,
            cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        var loaded = await service.GetAsync(created.Id, cancellationToken);
        return Success(PatientMedicalCertificateService.MapDetail(loaded), "Certificado guardado.");
    }

    private async Task<bool> CanEditAsync()
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        return IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.Create))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.Edit))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Create));
    }

    private Response<PatientMedicalCertificateDTO>? ValidateDoctorAssignment(int? requestedDoctorId, int? existingDoctorId)
    {
        if (IsAdmin
            || !CurrentDoctorId.HasValue
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("medicos", AdminPermissionCatalog.Actions.View)))
            return null;

        if (existingDoctorId.HasValue && existingDoctorId.Value != CurrentDoctorId.Value)
            return Unauthorized<PatientMedicalCertificateDTO>("No puede emitir o editar certificados firmados por otro médico.");

        var targetDoctorId = requestedDoctorId ?? CurrentDoctorId.Value;
        return targetDoctorId == CurrentDoctorId.Value
            ? null
            : Unauthorized<PatientMedicalCertificateDTO>("No puede seleccionar otro médico firmante para este certificado.");
    }

    private static PatientMedicalCertificateUpsertDTO ToUpsert(CreatePatientMedicalCertificateCommand request, int? currentDoctorId) => new()
    {
        DoctorId = request.DoctorId ?? currentDoctorId,
        IssuedAt = request.IssuedAt,
        CertificateType = request.CertificateType,
        Diagnosis = request.Diagnosis,
        Recommendation = request.Recommendation,
        IncludesRest = request.IncludesRest,
        RestStartDate = request.RestStartDate,
        RestEndDate = request.RestEndDate,
        RestDays = request.RestDays,
        Observations = request.Observations,
        Purpose = request.Purpose
    };
}

public class UpdatePatientMedicalCertificateCommandHandler : AppRequestHandler<UpdatePatientMedicalCertificateCommand, PatientMedicalCertificateDTO>
{
    private readonly IPatientMedicalCertificateService service;
    private readonly IUnitOfWork unitOfWork;

    public UpdatePatientMedicalCertificateCommandHandler(
        ICurrentUserService currentUserService,
        IPatientMedicalCertificateService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<PatientMedicalCertificateDTO>> Handle(
        UpdatePatientMedicalCertificateCommand request,
        CancellationToken cancellationToken)
    {
        if (!await CanEditAsync())
            return Unauthorized<PatientMedicalCertificateDTO>("Solo personal clínico autorizado puede editar certificados.");

        var existing = await service.GetAsync(request.CertificateId, cancellationToken);
        var doctorError = ValidateDoctorAssignment(request.DoctorId, existing.DoctorId);
        if (doctorError != null)
            return doctorError;

        var updated = await service.UpdateAsync(
            request.CertificateId,
            ToUpsert(request, CurrentDoctorId),
            CurrentDoctorId,
            CurrentUserId,
            cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        var loaded = await service.GetAsync(updated.Id, cancellationToken);
        return Success(PatientMedicalCertificateService.MapDetail(loaded), "Certificado actualizado.");
    }

    private async Task<bool> CanEditAsync()
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        return IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.Edit))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Edit));
    }

    private Response<PatientMedicalCertificateDTO>? ValidateDoctorAssignment(int? requestedDoctorId, int? existingDoctorId)
    {
        if (IsAdmin
            || !CurrentDoctorId.HasValue
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("medicos", AdminPermissionCatalog.Actions.View)))
            return null;

        if (existingDoctorId.HasValue && existingDoctorId.Value != CurrentDoctorId.Value)
            return Unauthorized<PatientMedicalCertificateDTO>("No puede editar certificados firmados por otro médico.");

        var targetDoctorId = requestedDoctorId ?? CurrentDoctorId.Value;
        return targetDoctorId == CurrentDoctorId.Value
            ? null
            : Unauthorized<PatientMedicalCertificateDTO>("No puede seleccionar otro médico firmante para este certificado.");
    }

    private static PatientMedicalCertificateUpsertDTO ToUpsert(UpdatePatientMedicalCertificateCommand request, int? currentDoctorId) => new()
    {
        DoctorId = request.DoctorId ?? currentDoctorId,
        IssuedAt = request.IssuedAt,
        CertificateType = request.CertificateType,
        Diagnosis = request.Diagnosis,
        Recommendation = request.Recommendation,
        IncludesRest = request.IncludesRest,
        RestStartDate = request.RestStartDate,
        RestEndDate = request.RestEndDate,
        RestDays = request.RestDays,
        Observations = request.Observations,
        Purpose = request.Purpose
    };
}

public class DeletePatientMedicalCertificateCommandHandler : AppRequestHandler<DeletePatientMedicalCertificateCommand, string>
{
    private readonly IPatientMedicalCertificateService service;
    private readonly IUnitOfWork unitOfWork;

    public DeletePatientMedicalCertificateCommandHandler(
        ICurrentUserService currentUserService,
        IPatientMedicalCertificateService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<string>> Handle(
        DeletePatientMedicalCertificateCommand request,
        CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var canDelete = IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("certificados", AdminPermissionCatalog.Actions.Delete));
        if (!canDelete)
            return Unauthorized<string>("No tiene permiso para anular certificados.");

        await service.DeleteAsync(request.CertificateId, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        return Deleted<string>("Certificado anulado.");
    }
}
