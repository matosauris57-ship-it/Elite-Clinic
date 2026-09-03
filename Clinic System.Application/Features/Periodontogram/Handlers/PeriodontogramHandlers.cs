namespace Clinic_System.Application.Features.Periodontogram.Handlers;

public class ListPeriodontalExamsQueryHandler : AppRequestHandler<ListPeriodontalExamsQuery, List<PeriodontalExamSummaryDTO>>
{
    private readonly IPeriodontalExamService service;

    public ListPeriodontalExamsQueryHandler(ICurrentUserService currentUserService, IPeriodontalExamService service)
        : base(currentUserService)
    {
        this.service = service;
    }

    public override async Task<Response<List<PeriodontalExamSummaryDTO>>> Handle(
        ListPeriodontalExamsQuery request,
        CancellationToken cancellationToken)
    {
        var (patientId, error) = await AuthorizeView(request.PatientId);
        if (error != null)
            return error;

        var exams = await service.ListByPatientAsync(patientId, cancellationToken);
        var latestId = exams.FirstOrDefault()?.Id;
        return Success(exams.Select(x => PeriodontalExamService.MapSummary(x, x.Id == latestId)).ToList());
    }

    private async Task<(int PatientId, Response<List<PeriodontalExamSummaryDTO>>? Error)> AuthorizeView(int patientId)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var permission = AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.View);
        var odontogramView = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.View);
        var (id, error) = await GetAuthorizedPatientId(patientId, permission);
        if (error != null && (roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(odontogramView)))
            return (patientId, null);
        return (id, error);
    }
}

public class GetPeriodontalExamQueryHandler : AppRequestHandler<GetPeriodontalExamQuery, PeriodontalExamDTO>
{
    private readonly IPeriodontalExamService service;

    public GetPeriodontalExamQueryHandler(ICurrentUserService currentUserService, IPeriodontalExamService service)
        : base(currentUserService)
    {
        this.service = service;
    }

    public override async Task<Response<PeriodontalExamDTO>> Handle(GetPeriodontalExamQuery request, CancellationToken cancellationToken)
    {
        var exam = await service.GetChartAsync(request.ExamId, cancellationToken);
        var viewError = await AuthorizePatientView(exam.PatientId);
        if (viewError != null)
            return viewError;

        var latest = (await service.ListByPatientAsync(exam.PatientId, cancellationToken)).FirstOrDefault();
        return Success(PeriodontalExamService.MapChart(exam, latest?.Id == exam.Id));
    }

    private async Task<Response<PeriodontalExamDTO>?> AuthorizePatientView(int patientId)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var permission = AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.View);
        var (_, error) = await GetAuthorizedPatientId(patientId, permission);
        if (error != null && (roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.View))))
            return null;
        return error;
    }
}

public class ComparePeriodontalExamsQueryHandler : AppRequestHandler<ComparePeriodontalExamsQuery, PeriodontalCompareDTO>
{
    private readonly IPeriodontalExamService service;

    public ComparePeriodontalExamsQueryHandler(ICurrentUserService currentUserService, IPeriodontalExamService service)
        : base(currentUserService)
    {
        this.service = service;
    }

    public override async Task<Response<PeriodontalCompareDTO>> Handle(
        ComparePeriodontalExamsQuery request,
        CancellationToken cancellationToken)
    {
        var comparison = await service.CompareAsync(request.PreviousExamId, request.CurrentExamId, cancellationToken);
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var permission = AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.View);
        var (_, error) = await GetAuthorizedPatientId(comparison.Current.PatientId, permission);
        if (error != null && !(roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase)
            || _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.View))))
            return error;
        return Success(comparison);
    }
}

public class CreatePeriodontalExamCommandHandler : AppRequestHandler<CreatePeriodontalExamCommand, PeriodontalExamDTO>
{
    private readonly IPeriodontalExamService service;
    private readonly IUnitOfWork unitOfWork;

    public CreatePeriodontalExamCommandHandler(
        ICurrentUserService currentUserService,
        IPeriodontalExamService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<PeriodontalExamDTO>> Handle(
        CreatePeriodontalExamCommand request,
        CancellationToken cancellationToken)
    {
        var editError = await AuthorizeEdit(request.PatientId);
        if (editError != null)
            return editError;

        var exam = await service.CreateAsync(
            request.PatientId,
            request.CopyLatest,
            CurrentDoctorId,
            CurrentUserId,
            cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        var loaded = await service.GetChartAsync(exam.Id, cancellationToken);
        return Success(PeriodontalExamService.MapChart(loaded, true), "Evaluación periodontal creada.");
    }

    private async Task<Response<PeriodontalExamDTO>?> AuthorizeEdit(int patientId)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var isClinician = IsAdmin ||
            roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.Edit)) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit));
        if (!isClinician)
            return Unauthorized<PeriodontalExamDTO>("Solo personal clínico autorizado puede registrar evaluaciones periodontales.");

        var permission = AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.Edit);
        var (_, error) = await GetAuthorizedPatientId(patientId, permission);
        if (error != null && roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
            return null;
        return error;
    }
}

public class SavePeriodontalExamCommandHandler : AppRequestHandler<SavePeriodontalExamCommand, PeriodontalExamDTO>
{
    private readonly IPeriodontalExamService service;
    private readonly IUnitOfWork unitOfWork;

    public SavePeriodontalExamCommandHandler(
        ICurrentUserService currentUserService,
        IPeriodontalExamService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<PeriodontalExamDTO>> Handle(
        SavePeriodontalExamCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await service.GetChartAsync(request.ExamId, cancellationToken);
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var isClinician = IsAdmin ||
            roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.Edit)) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit));
        if (!isClinician)
            return Unauthorized<PeriodontalExamDTO>("Solo personal clínico autorizado puede guardar el periodontograma.");

        var permission = AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.Edit);
        var (_, accessError) = await GetAuthorizedPatientId(existing.PatientId, permission);
        if (accessError != null && !roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
            return accessError;

        var exam = await service.SaveAsync(
            request.ExamId,
            new PeriodontalExamUpsertDTO
            {
                ExaminedAt = request.ExaminedAt,
                Notes = request.Notes,
                Teeth = request.Teeth
            },
            CurrentUserId,
            cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        var loaded = await service.GetChartAsync(exam.Id, cancellationToken);
        return Success(PeriodontalExamService.MapChart(loaded, true), "Evaluación periodontal guardada.");
    }
}

public class DeletePeriodontalExamCommandHandler : AppRequestHandler<DeletePeriodontalExamCommand, string>
{
    private readonly IPeriodontalExamService service;
    private readonly IUnitOfWork unitOfWork;

    public DeletePeriodontalExamCommandHandler(
        ICurrentUserService currentUserService,
        IPeriodontalExamService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<string>> Handle(DeletePeriodontalExamCommand request, CancellationToken cancellationToken)
    {
        var existing = await service.GetChartAsync(request.ExamId, cancellationToken);
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var isClinician = IsAdmin ||
            roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.Edit));
        if (!isClinician)
            return Unauthorized<string>("No tiene permiso para eliminar evaluaciones periodontales.");

        var permission = AdminPermissionCatalog.Build("periodontograma", AdminPermissionCatalog.Actions.Edit);
        var (_, accessError) = await GetAuthorizedPatientId(existing.PatientId, permission);
        if (accessError != null && !roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
            return Unauthorized<string>("Acceso denegado.");

        await service.DeleteAsync(request.ExamId, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        return Deleted<string>("Evaluación periodontal eliminada.");
    }
}
