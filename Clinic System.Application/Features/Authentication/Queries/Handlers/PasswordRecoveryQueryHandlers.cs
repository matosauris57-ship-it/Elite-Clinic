using Clinic_System.Core.Authorization;

namespace Clinic_System.Application.Features.Authentication.Queries.Handlers;

public class GetPasswordRecoveryRequestsQueryHandler : AppRequestHandler<GetPasswordRecoveryRequestsQuery, PasswordRecoveryListDTO>
{
    private readonly IPasswordRecoveryService _service;

    public GetPasswordRecoveryRequestsQueryHandler(
        ICurrentUserService currentUserService,
        IPasswordRecoveryService service) : base(currentUserService)
    {
        _service = service;
    }

    public override async Task<Response<PasswordRecoveryListDTO>> Handle(
        GetPasswordRecoveryRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!CanView())
            return Unauthorized<PasswordRecoveryListDTO>("No tiene permiso para ver solicitudes de contraseña.");

        PasswordRecoveryStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<PasswordRecoveryStatus>(request.Status, true, out var parsed)
            && parsed is PasswordRecoveryStatus.Pending or PasswordRecoveryStatus.Resolved or PasswordRecoveryStatus.Dismissed)
        {
            status = parsed;
        }

        var items = await _service.ListAsync(status, cancellationToken);
        var pending = await _service.CountPendingAsync(cancellationToken);
        return Success(new PasswordRecoveryListDTO
        {
            PendingCount = pending,
            Items = items.Select(Map).ToList()
        });
    }

    private bool CanView() =>
        IsAdmin || _currentUserService.HasPermission(AdminPermissionCatalog.Build("recuperacion-contrasena", AdminPermissionCatalog.Actions.View));

    internal static PasswordRecoveryRequestDTO Map(PasswordRecoveryRequest item) => new()
    {
        Id = item.Id,
        Identifier = item.Identifier,
        Comment = item.Comment,
        UserMatched = item.UserMatched,
        UserId = item.UserId,
        UserEmail = item.UserEmail,
        UserName = item.UserName,
        UserDisplayName = item.UserDisplayName,
        UserType = item.UserType,
        Status = item.Status.ToString(),
        RequestedAt = item.RequestedAt,
        ResolvedAt = item.ResolvedAt,
        ResolvedByName = item.ResolvedByName,
        ResolutionNote = item.ResolutionNote
    };
}

public class GetPasswordRecoveryPendingCountQueryHandler : AppRequestHandler<GetPasswordRecoveryPendingCountQuery, int>
{
    private readonly IPasswordRecoveryService _service;

    public GetPasswordRecoveryPendingCountQueryHandler(
        ICurrentUserService currentUserService,
        IPasswordRecoveryService service) : base(currentUserService)
    {
        _service = service;
    }

    public override async Task<Response<int>> Handle(GetPasswordRecoveryPendingCountQuery request, CancellationToken cancellationToken)
    {
        if (!IsAdmin && !_currentUserService.HasPermission(AdminPermissionCatalog.Build("recuperacion-contrasena", AdminPermissionCatalog.Actions.View)))
            return Unauthorized<int>("No tiene permiso para ver solicitudes de contraseña.");

        return Success(await _service.CountPendingAsync(cancellationToken));
    }
}

public class ResolvePasswordRecoveryCommandHandler : AppRequestHandler<ResolvePasswordRecoveryCommand, PasswordRecoveryRequestDTO>
{
    private readonly IPasswordRecoveryService _service;
    private readonly IIdentityService _identityService;

    public ResolvePasswordRecoveryCommandHandler(
        ICurrentUserService currentUserService,
        IPasswordRecoveryService service,
        IIdentityService identityService) : base(currentUserService)
    {
        _service = service;
        _identityService = identityService;
    }

    public override async Task<Response<PasswordRecoveryRequestDTO>> Handle(
        ResolvePasswordRecoveryCommand request, CancellationToken cancellationToken)
    {
        if (!IsAdmin && !_currentUserService.HasPermission(AdminPermissionCatalog.Build("recuperacion-contrasena", AdminPermissionCatalog.Actions.Edit)))
            return Unauthorized<PasswordRecoveryRequestDTO>("No tiene permiso para atender estas solicitudes.");

        try
        {
            var name = await _identityService.GetUserNameAsync(CurrentUserId, cancellationToken) ?? "Staff";
            var item = await _service.ResolveAsync(request.Id, request.Dismiss, CurrentUserId ?? string.Empty, name, request.Note, cancellationToken);
            return Success(GetPasswordRecoveryRequestsQueryHandler.Map(item), request.Dismiss ? "Solicitud descartada." : "Solicitud marcada como atendida.");
        }
        catch (NotFoundException ex)
        {
            return NotFound<PasswordRecoveryRequestDTO>(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest<PasswordRecoveryRequestDTO>(ex.Message);
        }
    }
}
