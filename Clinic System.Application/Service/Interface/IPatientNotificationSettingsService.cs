namespace Clinic_System.Application.Service.Interface
{
    public interface IPatientNotificationSettingsService
    {
        PatientNotificationSettings Get();
        Task SaveAsync(PatientNotificationSettings settings, CancellationToken cancellationToken = default);
    }

    public interface IPatientNotificationDispatchService
    {
        Task DispatchDueAsync();
    }

    public interface IEmailCampaignService
    {
        Task<EmailCampaignAudienceDTO> GetAudienceAsync(CancellationToken cancellationToken = default);
        Task<List<EmailCampaignListItemDTO>> ListAsync(CancellationToken cancellationToken = default);
        Task<EmailCampaignDetailDTO?> GetAsync(int id, CancellationToken cancellationToken = default);
        Task<(EmailCampaignDetailDTO? Data, string? Error)> CreateAsync(CreateEmailCampaignDTO request, CancellationToken cancellationToken = default);
        Task<(EmailCampaignDetailDTO? Data, string? Error)> UpdateDraftAsync(int id, CreateEmailCampaignDTO request, CancellationToken cancellationToken = default);
        Task<(EmailCampaignDetailDTO? Data, string? Error)> StartAsync(int id, CancellationToken cancellationToken = default);
        Task<(EmailCampaignDetailDTO? Data, string? Error)> PauseAsync(int id, CancellationToken cancellationToken = default);
        Task<(EmailCampaignDetailDTO? Data, string? Error)> ResumeAsync(int id, CancellationToken cancellationToken = default);
        Task<(EmailCampaignDetailDTO? Data, string? Error)> CancelAsync(int id, CancellationToken cancellationToken = default);
        Task DispatchDueAsync();
    }
}
