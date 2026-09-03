using Clinic_System.Application.DTOs.EmailCampaigns;
using Clinic_System.Core.Enums;
using Clinic_System.Core.Validation;
using Hangfire;
using MailKit.Net.Smtp;

namespace Clinic_System.Infrastructure.Services.Email;

public class EmailCampaignService : IEmailCampaignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IEmailSettingsProvider _emailSettings;
    private readonly ILogger<EmailCampaignService> _logger;

    public EmailCampaignService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IEmailSettingsProvider emailSettings,
        ILogger<EmailCampaignService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _emailSettings = emailSettings;
        _logger = logger;
    }

    public async Task<EmailCampaignAudienceDTO> GetAudienceAsync(CancellationToken cancellationToken = default)
    {
        var (withEmail, optedOut, invalid, eligible) =
            await _unitOfWork.PatientsRepository.CountEmailCampaignAudienceAsync(cancellationToken);
        var batches = eligible == 0 ? 0 : (int)Math.Ceiling(eligible / (double)EmailCampaignLimits.BatchSize);
        return new EmailCampaignAudienceDTO
        {
            WithEmailCount = withEmail,
            OptedOutCount = optedOut,
            InvalidCount = invalid,
            EligibleCount = eligible,
            BatchSize = EmailCampaignLimits.BatchSize,
            EstimatedBatches = batches,
            SmtpConfigured = _emailSettings.IsConfigured()
        };
    }

    public async Task<List<EmailCampaignListItemDTO>> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.EmailCampaignsRepository.ListAsync(cancellationToken);
        return items.Select(ToListItem).ToList();
    }

    public async Task<EmailCampaignDetailDTO?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var campaign = await _unitOfWork.EmailCampaignsRepository.GetWithRecipientsAsync(id, cancellationToken);
        return campaign == null ? null : ToDetail(campaign);
    }

    public async Task<(EmailCampaignDetailDTO? Data, string? Error)> CreateAsync(
        CreateEmailCampaignDTO request,
        CancellationToken cancellationToken = default)
    {
        var error = ValidateContent(request);
        if (error != null)
            return (null, error);

        var campaign = new EmailCampaign
        {
            Name = request.Name.Trim(),
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim(),
            Status = EmailCampaignStatus.Draft,
            BatchSize = EmailCampaignLimits.BatchSize
        };
        await _unitOfWork.EmailCampaignsRepository.AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        return (ToDetail(campaign), null);
    }

    public async Task<(EmailCampaignDetailDTO? Data, string? Error)> UpdateDraftAsync(
        int id,
        CreateEmailCampaignDTO request,
        CancellationToken cancellationToken = default)
    {
        var campaign = await _unitOfWork.EmailCampaignsRepository.GetTrackedAsync(id, cancellationToken);
        if (campaign == null)
            return (null, "No se encontró la campaña.");
        if (campaign.Status != EmailCampaignStatus.Draft)
            return (null, "Solo se puede editar una campaña en borrador.");

        var error = ValidateContent(request);
        if (error != null)
            return (null, error);

        campaign.Name = request.Name.Trim();
        campaign.Subject = request.Subject.Trim();
        campaign.Body = request.Body.Trim();
        _unitOfWork.EmailCampaignsRepository.Update(campaign);
        await _unitOfWork.SaveAsync(cancellationToken);
        return (ToDetail(campaign), null);
    }

    public async Task<(EmailCampaignDetailDTO? Data, string? Error)> StartAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_emailSettings.IsConfigured())
            return (null, "Configure el SMTP en Configuración → Datos de la clínica.");

        var campaign = await _unitOfWork.EmailCampaignsRepository.GetTrackedAsync(id, cancellationToken);
        if (campaign == null)
            return (null, "No se encontró la campaña.");
        if (campaign.Status != EmailCampaignStatus.Draft)
            return (null, "La campaña ya fue iniciada.");

        var audience = await _unitOfWork.PatientsRepository.GetEmailCampaignAudienceAsync(cancellationToken);
        var recipients = new List<EmailCampaignRecipient>();
        foreach (var patient in audience)
        {
            if (!ContactEmail.TryValidate(patient.Email, out var email, out _) || string.IsNullOrWhiteSpace(email))
                continue;

            recipients.Add(new EmailCampaignRecipient
            {
                EmailCampaignId = campaign.Id,
                PatientId = patient.Id,
                Email = email,
                PatientName = patient.FullName,
                Status = EmailCampaignRecipientStatus.Pending
            });
        }

        if (recipients.Count == 0)
            return (null, "No hay pacientes con correo válido para esta campaña.");

        await _unitOfWork.EmailCampaignsRepository.AddRecipientsAsync(recipients, cancellationToken);
        campaign.Status = EmailCampaignStatus.Running;
        campaign.StartedAt = DateTime.Now;
        campaign.RecipientCount = recipients.Count;
        campaign.SentCount = 0;
        campaign.FailedCount = 0;
        campaign.SkippedCount = 0;
        campaign.CompletedAt = null;
        _unitOfWork.EmailCampaignsRepository.Update(campaign);
        await _unitOfWork.SaveAsync(cancellationToken);

        _logger.LogInformation("Campaña {CampaignId} iniciada con {Count} destinatarios.", campaign.Id, recipients.Count);
        return (await GetAsync(id, cancellationToken), null);
    }

    public async Task<(EmailCampaignDetailDTO? Data, string? Error)> PauseAsync(int id, CancellationToken cancellationToken = default)
    {
        var campaign = await _unitOfWork.EmailCampaignsRepository.GetTrackedAsync(id, cancellationToken);
        if (campaign == null)
            return (null, "No se encontró la campaña.");
        if (campaign.Status != EmailCampaignStatus.Running)
            return (null, "Solo se puede pausar una campaña en curso.");

        campaign.Status = EmailCampaignStatus.Paused;
        _unitOfWork.EmailCampaignsRepository.Update(campaign);
        await _unitOfWork.SaveAsync(cancellationToken);
        return (await GetAsync(id, cancellationToken), null);
    }

    public async Task<(EmailCampaignDetailDTO? Data, string? Error)> ResumeAsync(int id, CancellationToken cancellationToken = default)
    {
        var campaign = await _unitOfWork.EmailCampaignsRepository.GetTrackedAsync(id, cancellationToken);
        if (campaign == null)
            return (null, "No se encontró la campaña.");
        if (campaign.Status != EmailCampaignStatus.Paused)
            return (null, "Solo se puede reanudar una campaña pausada.");
        if (campaign.SentCount + campaign.FailedCount + campaign.SkippedCount >= campaign.RecipientCount)
        {
            campaign.Status = EmailCampaignStatus.Completed;
            campaign.CompletedAt = DateTime.Now;
        }
        else
        {
            campaign.Status = EmailCampaignStatus.Running;
        }

        _unitOfWork.EmailCampaignsRepository.Update(campaign);
        await _unitOfWork.SaveAsync(cancellationToken);
        return (await GetAsync(id, cancellationToken), null);
    }

    public async Task<(EmailCampaignDetailDTO? Data, string? Error)> CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        var tracked = await _unitOfWork.EmailCampaignsRepository.GetTrackedAsync(id, cancellationToken);
        if (tracked == null)
            return (null, "No se encontró la campaña.");
        if (tracked.Status is EmailCampaignStatus.Completed or EmailCampaignStatus.Cancelled)
            return (null, "La campaña ya está cerrada.");

        var pending = await _unitOfWork.EmailCampaignsRepository.GetPendingByCampaignAsync(id, cancellationToken);
        foreach (var recipient in pending)
        {
            recipient.Status = EmailCampaignRecipientStatus.Skipped;
            recipient.Error = "Campaña cancelada.";
        }

        tracked.SkippedCount += pending.Count;
        tracked.Status = EmailCampaignStatus.Cancelled;
        tracked.CompletedAt = DateTime.Now;
        _unitOfWork.EmailCampaignsRepository.Update(tracked);
        await _unitOfWork.SaveAsync(cancellationToken);
        return (await GetAsync(id, cancellationToken), null);
    }

    [DisableConcurrentExecution(600)]
    public async Task DispatchDueAsync()
    {
        if (!_emailSettings.IsConfigured())
            return;

        var pending = await _unitOfWork.EmailCampaignsRepository.TakePendingAsync(EmailCampaignLimits.BatchSize);
        if (pending.Count == 0)
            return;

        var clinic = string.IsNullOrWhiteSpace(_emailSettings.Get().SenderName)
            ? "la clínica"
            : _emailSettings.Get().SenderName.Trim();

        var first = true;
        foreach (var recipient in pending)
        {
            if (!first)
                await Task.Delay(EmailCampaignLimits.SendPause);
            first = false;

            var campaign = recipient.EmailCampaign;
            if (campaign.Status != EmailCampaignStatus.Running)
                continue;

            var patient = recipient.Patient;
            if (patient.OptOutEmailCampaigns || patient.EmailInvalid
                || !ContactEmail.TryValidate(recipient.Email, out var to, out _) || string.IsNullOrWhiteSpace(to))
            {
                recipient.Status = EmailCampaignRecipientStatus.Skipped;
                recipient.Error = patient.OptOutEmailCampaigns
                    ? "El paciente no desea campañas."
                    : patient.EmailInvalid
                        ? "Correo marcado como inválido."
                        : "Correo no válido.";
                campaign.SkippedCount++;
                MarkCompletedIfDone(campaign);
                continue;
            }

            try
            {
                var subject = EmailCampaignLimits.Apply(campaign.Subject, clinic, recipient.PatientName);
                var body = EmailCampaignLimits.AppendFooter(
                    EmailCampaignLimits.Apply(campaign.Body, clinic, recipient.PatientName),
                    clinic);
                await _emailService.SendEmailAsync(to, subject, body);
                recipient.Status = EmailCampaignRecipientStatus.Sent;
                recipient.SentAt = DateTime.Now;
                recipient.Error = null;
                campaign.SentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Campaña {CampaignId}: no se pudo enviar a {Email}", campaign.Id, to);
                recipient.Status = EmailCampaignRecipientStatus.Failed;
                recipient.Error = Truncate(ex.Message, 500);
                campaign.FailedCount++;
                if (IsPermanentFailure(ex))
                {
                    patient.EmailInvalid = true;
                    _unitOfWork.PatientsRepository.Update(patient);
                }
            }

            MarkCompletedIfDone(campaign);
        }

        foreach (var campaign in pending.Select(x => x.EmailCampaign).DistinctBy(x => x.Id))
            _unitOfWork.EmailCampaignsRepository.Update(campaign);

        await _unitOfWork.SaveAsync();
        _logger.LogInformation("Lote de campaña: {Count} correos procesados.", pending.Count);
    }

    private static void MarkCompletedIfDone(EmailCampaign campaign)
    {
        var processed = campaign.SentCount + campaign.FailedCount + campaign.SkippedCount;
        if (processed < campaign.RecipientCount)
            return;
        campaign.Status = EmailCampaignStatus.Completed;
        campaign.CompletedAt ??= DateTime.Now;
    }

    private static string? ValidateContent(CreateEmailCampaignDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Indique un nombre para la campaña.";
        if (request.Name.Trim().Length > EmailCampaignLimits.NameMaxLength)
            return $"El nombre no puede superar {EmailCampaignLimits.NameMaxLength} caracteres.";
        if (string.IsNullOrWhiteSpace(request.Subject))
            return "Indique el asunto del correo.";
        if (request.Subject.Trim().Length > EmailCampaignLimits.SubjectMaxLength)
            return $"El asunto no puede superar {EmailCampaignLimits.SubjectMaxLength} caracteres.";
        if (string.IsNullOrWhiteSpace(request.Body))
            return "Escriba el mensaje.";
        if (request.Body.Trim().Length > EmailCampaignLimits.BodyMaxLength)
            return $"El mensaje no puede superar {EmailCampaignLimits.BodyMaxLength} caracteres.";
        return null;
    }

    private static EmailCampaignListItemDTO ToListItem(EmailCampaign campaign) => new()
    {
        Id = campaign.Id,
        Name = campaign.Name,
        Subject = campaign.Subject,
        Status = campaign.Status.ToString(),
        RecipientCount = campaign.RecipientCount,
        SentCount = campaign.SentCount,
        FailedCount = campaign.FailedCount,
        SkippedCount = campaign.SkippedCount,
        PendingCount = Math.Max(0, campaign.RecipientCount - campaign.SentCount - campaign.FailedCount - campaign.SkippedCount),
        CreatedAt = campaign.CreatedAt,
        StartedAt = campaign.StartedAt,
        CompletedAt = campaign.CompletedAt
    };

    private static EmailCampaignDetailDTO ToDetail(EmailCampaign campaign)
    {
        var dto = new EmailCampaignDetailDTO
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Subject = campaign.Subject,
            Body = campaign.Body,
            Status = campaign.Status.ToString(),
            BatchSize = campaign.BatchSize <= 0 ? EmailCampaignLimits.BatchSize : campaign.BatchSize,
            RecipientCount = campaign.RecipientCount,
            SentCount = campaign.SentCount,
            FailedCount = campaign.FailedCount,
            SkippedCount = campaign.SkippedCount,
            PendingCount = Math.Max(0, campaign.RecipientCount - campaign.SentCount - campaign.FailedCount - campaign.SkippedCount),
            CreatedAt = campaign.CreatedAt,
            StartedAt = campaign.StartedAt,
            CompletedAt = campaign.CompletedAt,
            RecentRecipients = (campaign.Recipients ?? [])
                .OrderByDescending(x => x.SentAt)
                .ThenByDescending(x => x.Id)
                .Take(40)
                .Select(x => new EmailCampaignRecipientDTO
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.PatientName,
                    Email = x.Email,
                    Status = x.Status.ToString(),
                    Error = x.Error,
                    SentAt = x.SentAt
                })
                .ToList()
        };
        return dto;
    }

    private static bool IsPermanentFailure(Exception ex)
    {
        if (ex is SmtpCommandException smtp)
        {
            if (smtp.ErrorCode is SmtpErrorCode.RecipientNotAccepted)
                return true;
            var code = (int)smtp.StatusCode;
            if (code is >= 550 and <= 559)
                return true;
        }

        var text = ex.ToString();
        return text.Contains("5.1.1", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Mailbox unavailable", StringComparison.OrdinalIgnoreCase)
            || text.Contains("User unknown", StringComparison.OrdinalIgnoreCase)
            || text.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value : value[..max];
}
