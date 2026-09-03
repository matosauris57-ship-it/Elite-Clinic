namespace Clinic_System.Application.Features.PatientPrescriptions.Handlers;

public class ListPrescriptionTemplatesQueryHandler : AppRequestHandler<ListPrescriptionTemplatesQuery, List<PrescriptionTemplateDTO>>
{
    private readonly IPatientPrescriptionService service;

    public ListPrescriptionTemplatesQueryHandler(ICurrentUserService currentUserService, IPatientPrescriptionService service)
        : base(currentUserService)
    {
        this.service = service;
    }

    public override Task<Response<List<PrescriptionTemplateDTO>>> Handle(
        ListPrescriptionTemplatesQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(Success(service.ListTemplates().ToList()));
}

public class ListPatientPrescriptionsQueryHandler : AppRequestHandler<ListPatientPrescriptionsQuery, List<PatientPrescriptionSummaryDTO>>
{
    private readonly IPatientPrescriptionService service;

    public ListPatientPrescriptionsQueryHandler(ICurrentUserService currentUserService, IPatientPrescriptionService service)
        : base(currentUserService)
    {
        this.service = service;
    }

    public override async Task<Response<List<PatientPrescriptionSummaryDTO>>> Handle(
        ListPatientPrescriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var error = await AuthorizeView(request.PatientId);
        if (error != null)
            return error;

        var recetas = await service.ListByPatientAsync(request.PatientId, cancellationToken);
        return Success(recetas.Select(PatientPrescriptionService.MapSummary).ToList());
    }

    private async Task<Response<List<PatientPrescriptionSummaryDTO>>?> AuthorizeView(int patientId)
    {
        if (await CanViewAsync())
            return null;
        var (_, error) = await GetAuthorizedPatientId(patientId, AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.View));
        return error;
    }

    private async Task<bool> CanViewAsync()
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        return IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.View))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.View));
    }
}

public class GetPatientPrescriptionQueryHandler : AppRequestHandler<GetPatientPrescriptionQuery, PatientPrescriptionDTO>
{
    private readonly IPatientPrescriptionService service;
    private readonly IUnitOfWork unitOfWork;

    public GetPatientPrescriptionQueryHandler(
        ICurrentUserService currentUserService,
        IPatientPrescriptionService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<PatientPrescriptionDTO>> Handle(
        GetPatientPrescriptionQuery request,
        CancellationToken cancellationToken)
    {
        var prescription = await service.GetAsync(request.PrescriptionId, cancellationToken);
        var error = await AuthorizeView(prescription.PatientId);
        if (error != null)
            return error;

        return Success(await PatientPrescriptionService.MapDetailAsync(prescription, unitOfWork, cancellationToken));
    }

    private async Task<Response<PatientPrescriptionDTO>?> AuthorizeView(int patientId)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var canView = IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.View))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.View));
        if (canView)
            return null;
        var (_, error) = await GetAuthorizedPatientId(patientId, AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.View));
        return error;
    }
}

public class CreatePatientPrescriptionCommandHandler : AppRequestHandler<CreatePatientPrescriptionCommand, PatientPrescriptionDTO>
{
    private readonly IPatientPrescriptionService service;
    private readonly IUnitOfWork unitOfWork;

    public CreatePatientPrescriptionCommandHandler(
        ICurrentUserService currentUserService,
        IPatientPrescriptionService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<PatientPrescriptionDTO>> Handle(
        CreatePatientPrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        var editError = await AuthorizeEdit();
        if (editError != null)
            return editError;

        var created = await service.CreateAsync(
            request.PatientId,
            new PatientPrescriptionUpsertDTO
            {
                DoctorId = request.DoctorId ?? CurrentDoctorId,
                IssuedAt = request.IssuedAt,
                Diagnosis = request.Diagnosis,
                Notes = request.Notes,
                TemplateKeys = request.TemplateKeys,
                Items = request.Items
            },
            CurrentDoctorId,
            CurrentUserId,
            cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        var loaded = await service.GetAsync(created.Id, cancellationToken);
        return Success(await PatientPrescriptionService.MapDetailAsync(loaded, unitOfWork, cancellationToken), "Receta guardada.");
    }

    private async Task<Response<PatientPrescriptionDTO>?> AuthorizeEdit()
    {
        if (await CanEditAsync())
            return null;
        return Unauthorized<PatientPrescriptionDTO>("Solo personal clínico autorizado puede emitir recetas.");
    }

    private async Task<bool> CanEditAsync()
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        return IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.Create))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.Edit))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Create));
    }
}

public class UpdatePatientPrescriptionCommandHandler : AppRequestHandler<UpdatePatientPrescriptionCommand, PatientPrescriptionDTO>
{
    private readonly IPatientPrescriptionService service;
    private readonly IUnitOfWork unitOfWork;

    public UpdatePatientPrescriptionCommandHandler(
        ICurrentUserService currentUserService,
        IPatientPrescriptionService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<PatientPrescriptionDTO>> Handle(
        UpdatePatientPrescriptionCommand request,
        CancellationToken cancellationToken)
    {
        if (!await CanEditAsync())
            return Unauthorized<PatientPrescriptionDTO>("Solo personal clínico autorizado puede editar recetas.");

        var updated = await service.UpdateAsync(
            request.PrescriptionId,
            new PatientPrescriptionUpsertDTO
            {
                DoctorId = request.DoctorId ?? CurrentDoctorId,
                IssuedAt = request.IssuedAt,
                Diagnosis = request.Diagnosis,
                Notes = request.Notes,
                TemplateKeys = request.TemplateKeys,
                Items = request.Items
            },
            CurrentDoctorId,
            CurrentUserId,
            cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        var loaded = await service.GetAsync(updated.Id, cancellationToken);
        return Success(await PatientPrescriptionService.MapDetailAsync(loaded, unitOfWork, cancellationToken), "Receta actualizada.");
    }

    private async Task<bool> CanEditAsync()
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        return IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.Edit))
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("historial", AdminPermissionCatalog.Actions.Edit));
    }
}

public class DeletePatientPrescriptionCommandHandler : AppRequestHandler<DeletePatientPrescriptionCommand, string>
{
    private readonly IPatientPrescriptionService service;
    private readonly IUnitOfWork unitOfWork;

    public DeletePatientPrescriptionCommandHandler(
        ICurrentUserService currentUserService,
        IPatientPrescriptionService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<string>> Handle(DeletePatientPrescriptionCommand request, CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var canDelete = IsAdmin
            || roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("recetas", AdminPermissionCatalog.Actions.Delete));
        if (!canDelete)
            return Unauthorized<string>("No tiene permiso para anular recetas.");

        await service.DeleteAsync(request.PrescriptionId, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        return Deleted<string>("Receta anulada.");
    }
}
