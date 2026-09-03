namespace Clinic_System.Application.Features.ToothChart.Handlers;

public class CreateToothChartEntryCommandHandler : AppRequestHandler<CreateToothChartEntryCommand, ToothChartEntryDTO>
{
    private readonly IToothChartService service;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public CreateToothChartEntryCommandHandler(
        ICurrentUserService currentUserService,
        IToothChartService service,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public override async Task<Response<ToothChartEntryDTO>> Handle(CreateToothChartEntryCommand request, CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var isClinician = IsAdmin ||
            roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit));
        if (!isClinician)
            return Unauthorized<ToothChartEntryDTO>("Solo médicos o administradores autorizados pueden registrar entradas clínicas.");

        var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit);
        var (patientId, accessError) = await GetAuthorizedPatientId(request.PatientId, permission);
        if (accessError != null && roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
            patientId = request.PatientId;
        else if (accessError != null)
            return accessError;

        var teeth = ResolveTeeth(request.ToothNumber, request.ToothNumbers, request.BridgeUnits);
        if (teeth.Count == 0)
            return BadRequest<ToothChartEntryDTO>("Seleccione una o más piezas.");

        var (spanId, bridgeRoles) = ResolveBridge(request.Condition, request.BridgeSpanId, request.BridgeUnits);

        ToothChartEntry? last = null;
        foreach (var tooth in teeth)
        {
            var material = request.RestorationMaterial;
            if (material.HasValue && !RestorationMaterialRules.IsAllowed(tooth, request.Condition, material))
                material = null;

            bridgeRoles.TryGetValue(tooth, out var role);
            last = await service.CreateEntryAsync(
                patientId, tooth, request.Surface, request.Phase, request.Condition,
                request.Severity, request.Notes, request.AppointmentId, CurrentUserId,
                material, request.CariesType, request.Icdas,
                request.ClinicalDiagnosis, request.ProposedTreatment,
                spanId, request.Condition == ToothCondition.Bridge ? role : null,
                cancellationToken);
        }

        await unitOfWork.SaveAsync(cancellationToken);
        var message = teeth.Count == 1
            ? "Entrada clínica registrada."
            : $"Diagnóstico registrado en {teeth.Count} piezas.";
        return Success(mapper.Map<ToothChartEntryDTO>(last!), message);
    }

    private static List<int> ResolveTeeth(int toothNumber, List<int>? toothNumbers, List<BridgeUnitInput>? units)
    {
        if (units is { Count: > 0 })
            return units.Select(x => x.ToothNumber).Where(FdiToothNumber.IsValid).Distinct().ToList();

        var teeth = (toothNumbers ?? [])
            .Where(FdiToothNumber.IsValid)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        if (teeth.Count == 0 && FdiToothNumber.IsValid(toothNumber))
            teeth.Add(toothNumber);
        return teeth;
    }

    private static (Guid? SpanId, Dictionary<int, BridgeRole> Roles) ResolveBridge(
        ToothCondition condition,
        Guid? spanId,
        List<BridgeUnitInput>? units)
    {
        if (condition != ToothCondition.Bridge || units is not { Count: > 0 })
            return (null, []);

        return (spanId ?? Guid.NewGuid(), units.ToDictionary(x => x.ToothNumber, x => x.Role));
    }
}

public class CreateToothChartEntriesBatchCommandHandler : AppRequestHandler<CreateToothChartEntriesBatchCommand, List<ToothChartEntryDTO>>
{
    private readonly IToothChartService service;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public CreateToothChartEntriesBatchCommandHandler(
        ICurrentUserService currentUserService,
        IToothChartService service,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public override async Task<Response<List<ToothChartEntryDTO>>> Handle(
        CreateToothChartEntriesBatchCommand request,
        CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var isClinician = IsAdmin ||
            roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit));
        if (!isClinician)
            return Unauthorized<List<ToothChartEntryDTO>>("Solo médicos o administradores autorizados pueden registrar entradas clínicas.");

        var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit);
        var (patientId, accessError) = await GetAuthorizedPatientId(request.PatientId, permission);
        if (accessError != null && roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
            patientId = request.PatientId;
        else if (accessError != null)
            return accessError;

        var teeth = request.BridgeUnits.Count > 0
            ? request.BridgeUnits.Select(x => x.ToothNumber).Distinct().ToList()
            : request.ToothNumbers.Distinct().OrderBy(x => x).ToList();
        Guid? spanId = null;
        var bridgeRoles = new Dictionary<int, BridgeRole>();
        if (request.Condition == ToothCondition.Bridge && request.BridgeUnits.Count > 0)
        {
            spanId = request.BridgeSpanId ?? Guid.NewGuid();
            bridgeRoles = request.BridgeUnits.ToDictionary(x => x.ToothNumber, x => x.Role);
        }

        var created = new List<ToothChartEntry>();
        foreach (var tooth in teeth)
        {
            var material = request.RestorationMaterial;
            if (material.HasValue && !RestorationMaterialRules.IsAllowed(tooth, request.Condition, material))
                material = null;

            bridgeRoles.TryGetValue(tooth, out var role);
            created.Add(await service.CreateEntryAsync(
                patientId, tooth, request.Surface, request.Phase, request.Condition,
                request.Severity, request.Notes, request.AppointmentId, CurrentUserId,
                material, request.CariesType, request.Icdas,
                request.ClinicalDiagnosis, request.ProposedTreatment,
                spanId, request.Condition == ToothCondition.Bridge ? role : null,
                cancellationToken));
        }

        await unitOfWork.SaveAsync(cancellationToken);
        var message = created.Count == 1
            ? "Entrada clínica registrada."
            : $"Diagnóstico registrado en {created.Count} piezas.";
        return Success(mapper.Map<List<ToothChartEntryDTO>>(created), message);
    }
}

public class GetCurrentToothChartQueryHandler : AppRequestHandler<GetCurrentToothChartQuery, List<ToothChartEntryDTO>>
{
    private readonly IToothChartService service;
    private readonly IMapper mapper;

    public GetCurrentToothChartQueryHandler(ICurrentUserService currentUserService, IToothChartService service, IMapper mapper)
        : base(currentUserService)
    {
        this.service = service;
        this.mapper = mapper;
    }

    public override async Task<Response<List<ToothChartEntryDTO>>> Handle(GetCurrentToothChartQuery request, CancellationToken cancellationToken)
    {
        var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.View);
        var (patientId, error) = await GetAuthorizedPatientId(request.PatientId, permission);
        if (error != null && CurrentDoctorId.HasValue)
            patientId = request.PatientId;
        else if (error != null)
            return error;

        var entries = await service.GetCurrentAsync(patientId, request.Dentition, request.Quadrant, cancellationToken);
        return Success(mapper.Map<List<ToothChartEntryDTO>>(entries));
    }
}

public class GetDentalTimelineQueryHandler : AppRequestHandler<GetDentalTimelineQuery, List<DentalClinicalEventDTO>>
{
    private readonly IToothChartService service;
    private readonly IIdentityService identityService;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public GetDentalTimelineQueryHandler(
        ICurrentUserService currentUserService,
        IToothChartService service,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(currentUserService)
    {
        this.service = service;
        this.identityService = identityService;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public override async Task<Response<List<DentalClinicalEventDTO>>> Handle(GetDentalTimelineQuery request, CancellationToken cancellationToken)
    {
        var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.View);
        var (patientId, error) = await GetAuthorizedPatientId(request.PatientId, permission);
        if (error != null && CurrentDoctorId.HasValue)
            patientId = request.PatientId;
        else if (error != null)
            return error;

        var events = await service.GetTimelineAsync(patientId, request.ToothNumber, cancellationToken);
        var dtos = mapper.Map<List<DentalClinicalEventDTO>>(events);
        await EnrichActorsAsync(dtos, cancellationToken);
        foreach (var dto in dtos)
            dto.Title = ToothChartEventText.LocalizeTitle(dto.Title);
        return Success(dtos);
    }

    private async Task EnrichActorsAsync(List<DentalClinicalEventDTO> dtos, CancellationToken cancellationToken)
    {
        var userIds = dtos
            .Select(x => x.RecordedByUserId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (userIds.Count == 0)
            return;

        var names = (await identityService.GetUserDisplayNamesAsync(userIds, cancellationToken))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

        foreach (var userId in userIds)
        {
            var doctor = await unitOfWork.DoctorsRepository.GetDoctorByUserIdAsync(userId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(doctor?.FullName))
                names[userId] = doctor.FullName;
        }

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.RecordedByUserId))
                continue;
            if (names.TryGetValue(dto.RecordedByUserId, out var name))
                dto.RecordedByUserName = name;
        }
    }
}
