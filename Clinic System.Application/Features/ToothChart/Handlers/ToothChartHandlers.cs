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
        var surfaces = ToothSurfaceSelection.ResolveForWrite(request.Condition, request.Surface, request.Surfaces);

        ToothChartEntry? last = null;
        foreach (var tooth in teeth)
        {
            var material = request.RestorationMaterial;
            if (material.HasValue && !RestorationMaterialRules.IsAllowed(tooth, request.Condition, material))
                material = null;

            bridgeRoles.TryGetValue(tooth, out var role);
            foreach (var surface in surfaces)
            {
                last = await service.CreateEntryAsync(
                    patientId, tooth, surface, request.Phase, request.Condition,
                    request.Severity, request.Notes, request.AppointmentId, CurrentUserId,
                    material, request.CariesType, request.Icdas,
                    request.ClinicalDiagnosis, request.ProposedTreatment,
                    spanId, request.Condition == ToothCondition.Bridge ? role : null,
                    cancellationToken);
            }
        }

        await unitOfWork.SaveAsync(cancellationToken);
        return Success(
            mapper.Map<ToothChartEntryDTO>(last!),
            ToothSurfaceSelection.CreatedCountMessage(teeth.Count, surfaces.Count));
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

        var surfaces = ToothSurfaceSelection.ResolveForWrite(request.Condition, request.Surface, request.Surfaces);
        var created = new List<ToothChartEntry>();
        foreach (var tooth in teeth)
        {
            var material = request.RestorationMaterial;
            if (material.HasValue && !RestorationMaterialRules.IsAllowed(tooth, request.Condition, material))
                material = null;

            bridgeRoles.TryGetValue(tooth, out var role);
            foreach (var surface in surfaces)
            {
                created.Add(await service.CreateEntryAsync(
                    patientId, tooth, surface, request.Phase, request.Condition,
                    request.Severity, request.Notes, request.AppointmentId, CurrentUserId,
                    material, request.CariesType, request.Icdas,
                    request.ClinicalDiagnosis, request.ProposedTreatment,
                    spanId, request.Condition == ToothCondition.Bridge ? role : null,
                    cancellationToken));
            }
        }

        await unitOfWork.SaveAsync(cancellationToken);
        return Success(
            mapper.Map<List<ToothChartEntryDTO>>(created),
            ToothSurfaceSelection.CreatedCountMessage(teeth.Count, surfaces.Count));
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
        await EnrichToothChartEntryIdsAsync(patientId, dtos, cancellationToken);
        foreach (var dto in dtos)
            dto.Title = ToothChartEventText.LocalizeTitle(dto.Title);
        return Success(dtos);
    }

    private async Task EnrichToothChartEntryIdsAsync(
        int patientId,
        List<DentalClinicalEventDTO> dtos,
        CancellationToken cancellationToken)
    {
        if (!dtos.Any(x => x.Type == DentalClinicalEventType.OdontogramEntry))
            return;

        var entries = (await unitOfWork.ToothChartEntriesRepository.GetByPatientAsync(patientId, cancellationToken)).ToList();
        foreach (var dto in dtos)
        {
            if (dto.Type != DentalClinicalEventType.OdontogramEntry)
                continue;

            if (long.TryParse(dto.ReferenceId, out var parsedId))
            {
                var byId = entries.FirstOrDefault(e => e.Id == parsedId);
                if (byId != null)
                {
                    dto.ToothChartEntryId = parsedId;
                    if (byId.IsVoided)
                        dto.IsVoided = true;
                    continue;
                }
            }

            var match = entries
                .Where(e => dto.ToothNumber.HasValue && e.ToothNumber == dto.ToothNumber.Value)
                .OrderBy(e => Math.Abs((e.RecordedAt - dto.RecordedAt).TotalMilliseconds))
                .FirstOrDefault(e => Math.Abs((e.RecordedAt - dto.RecordedAt).TotalSeconds) < 3);
            if (match != null)
            {
                dto.ToothChartEntryId = match.Id;
                if (match.IsVoided)
                    dto.IsVoided = true;
            }
        }
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

public class GetToothChartEntryQueryHandler : AppRequestHandler<GetToothChartEntryQuery, ToothChartEntryDTO>
{
    private readonly IToothChartService service;
    private readonly IMapper mapper;

    public GetToothChartEntryQueryHandler(ICurrentUserService currentUserService, IToothChartService service, IMapper mapper)
        : base(currentUserService)
    {
        this.service = service;
        this.mapper = mapper;
    }

    public override async Task<Response<ToothChartEntryDTO>> Handle(GetToothChartEntryQuery request, CancellationToken cancellationToken)
    {
        var entry = await service.GetEntryAsync(request.Id, cancellationToken);
        if (entry == null)
            return NotFound<ToothChartEntryDTO>("No se encontró el hallazgo.");

        var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.View);
        var (_, error) = await GetAuthorizedPatientId(entry.PatientId, permission);
        if (error != null && !CurrentDoctorId.HasValue)
            return error;

        return Success(mapper.Map<ToothChartEntryDTO>(entry));
    }
}

public class UpdateToothChartEntryCommandHandler : AppRequestHandler<UpdateToothChartEntryCommand, ToothChartEntryDTO>
{
    private readonly IToothChartService service;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public UpdateToothChartEntryCommandHandler(
        ICurrentUserService currentUserService,
        IToothChartService service,
        IUnitOfWork unitOfWork,
        IMapper mapper) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public override async Task<Response<ToothChartEntryDTO>> Handle(UpdateToothChartEntryCommand request, CancellationToken cancellationToken)
    {
        var roles = await _currentUserService.GetCurrentUserRolesAsync();
        var isClinician = IsAdmin ||
            roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
            _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit));
        if (!isClinician)
            return Unauthorized<ToothChartEntryDTO>("Solo médicos o administradores autorizados pueden editar entradas clínicas.");

        var existing = await service.GetEntryAsync(request.Id, cancellationToken);
        if (existing == null)
            return NotFound<ToothChartEntryDTO>("No se encontró el hallazgo.");

        var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit);
        var (_, accessError) = await GetAuthorizedPatientId(existing.PatientId, permission);
        if (accessError != null && !roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
            return accessError;

        var material = request.RestorationMaterial;
        if (material.HasValue && !RestorationMaterialRules.IsAllowed(existing.ToothNumber, request.Condition, material))
            material = null;

        try
        {
            var updated = await service.UpdateEntryAsync(
                request.Id,
                request.Surface,
                request.Phase,
                request.Condition,
                request.Severity,
                request.Notes,
                material,
                request.CariesType,
                request.Icdas,
                request.ClinicalDiagnosis,
                request.ProposedTreatment,
                cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<ToothChartEntryDTO>(updated), "Hallazgo actualizado.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<ToothChartEntryDTO>(ex.Message);
        }
    }
}

public class VoidToothChartEntryCommandHandler : AppRequestHandler<VoidToothChartEntryCommand, string>
{
    private readonly IToothChartService service;
    private readonly IUnitOfWork unitOfWork;

    public VoidToothChartEntryCommandHandler(
        ICurrentUserService currentUserService,
        IToothChartService service,
        IUnitOfWork unitOfWork) : base(currentUserService)
    {
        this.service = service;
        this.unitOfWork = unitOfWork;
    }

    public override async Task<Response<string>> Handle(VoidToothChartEntryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            var isClinician = IsAdmin ||
                roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase) ||
                _currentUserService.HasPermission(AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit));
            if (!isClinician)
                return Unauthorized<string>("Solo médicos o administradores autorizados pueden anular entradas clínicas.");

            var existing = await service.GetEntryAsync(request.Id, cancellationToken);
            if (existing == null)
                return NotFound<string>("No se encontró el hallazgo.");

            var permission = AdminPermissionCatalog.Build("odontograma", AdminPermissionCatalog.Actions.Edit);
            var (_, accessError) = await GetAuthorizedPatientId(existing.PatientId, permission);
            if (accessError != null && !roles.Contains(AdminPermissionCatalog.SystemRoles.Doctor, StringComparer.OrdinalIgnoreCase))
                return accessError;

            await service.VoidEntryAsync(request.Id, CurrentUserId, null, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Success("Hallazgo anulado.", "Hallazgo anulado.");
        }
        catch (NotFoundException ex)
        {
            return NotFound<string>(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<string>(ex.Message);
        }
    }
}
