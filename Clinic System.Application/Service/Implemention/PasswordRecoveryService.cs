namespace Clinic_System.Application.Service.Implemention;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    public static readonly TimeSpan DuplicateWindow = TimeSpan.FromHours(12);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly INotificationsService _notifications;

    public PasswordRecoveryService(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        INotificationsService notifications)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _notifications = notifications;
    }

    public async Task<PasswordRecoveryRequest> SubmitAsync(string identifier, string? comment, CancellationToken cancellationToken = default)
    {
        var trimmed = identifier.Trim();
        var normalized = trimmed.ToUpperInvariant();
        var since = DateTime.Now.Subtract(DuplicateWindow);
        var existing = await _unitOfWork.PasswordRecoveryRequestsRepository
            .FindRecentPendingAsync(normalized, since, cancellationToken);
        if (existing != null)
            return existing;

        var match = await _identityService.FindAccountForRecoveryAsync(trimmed, cancellationToken);
        var note = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (note is { Length: > 500 })
            note = note[..500];

        var request = new PasswordRecoveryRequest
        {
            Identifier = trimmed,
            NormalizedIdentifier = normalized,
            Comment = note,
            RequestedAt = DateTime.Now,
            Status = PasswordRecoveryStatus.Pending,
            UserMatched = match != null,
            UserId = match?.UserId,
            UserEmail = match?.Email,
            UserName = match?.UserName,
            UserDisplayName = match?.DisplayName,
            UserType = match?.UserType
        };

        await _unitOfWork.PasswordRecoveryRequestsRepository.AddAsync(request, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        var who = request.UserMatched
            ? $"{request.UserDisplayName} ({request.UserEmail ?? request.Identifier})"
            : request.Identifier;

        await _notifications.SendToGroupAsync("Admins", new NotificationDTO
        {
            Title = "Olvidé mi contraseña",
            Message = $"{who} pidió recuperar el acceso. Revise Recuperación de contraseña.",
            NotificationType = "PasswordRecoveryRequested",
            RelatedEntityId = request.Id
        });
        await _notifications.SendToGroupAsync("PasswordRecoveryStaff", new NotificationDTO
        {
            Title = "Olvidé mi contraseña",
            Message = $"{who} pidió recuperar el acceso.",
            NotificationType = "PasswordRecoveryRequested",
            RelatedEntityId = request.Id
        });

        return request;
    }

    public Task<int> CountPendingAsync(CancellationToken cancellationToken = default) =>
        _unitOfWork.PasswordRecoveryRequestsRepository.CountPendingAsync(cancellationToken);

    public Task<List<PasswordRecoveryRequest>> ListAsync(PasswordRecoveryStatus? status, CancellationToken cancellationToken = default) =>
        _unitOfWork.PasswordRecoveryRequestsRepository.ListAsync(status, cancellationToken);

    public async Task<PasswordRecoveryRequest> ResolveAsync(
        int id, bool dismiss, string resolvedByUserId, string resolvedByName, string? note, CancellationToken cancellationToken = default)
    {
        var request = await _unitOfWork.PasswordRecoveryRequestsRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("No se encontró la solicitud.");

        if (!request.IsPending)
            throw new InvalidOperationException("Esta solicitud ya fue atendida.");

        if (dismiss)
            request.Dismiss(resolvedByUserId, resolvedByName, note);
        else
            request.MarkResolved(resolvedByUserId, resolvedByName, note);

        _unitOfWork.PasswordRecoveryRequestsRepository.Update(request, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        return request;
    }
}
