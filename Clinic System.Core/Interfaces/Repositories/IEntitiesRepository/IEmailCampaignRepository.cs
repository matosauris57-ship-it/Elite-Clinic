namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IEmailCampaignRepository : IGenericRepository<EmailCampaign>
{
    Task<List<EmailCampaign>> ListAsync(CancellationToken cancellationToken = default);
    Task<EmailCampaign?> GetWithRecipientsAsync(int id, CancellationToken cancellationToken = default);
    Task<EmailCampaign?> GetTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<List<EmailCampaignRecipient>> TakePendingAsync(int take, CancellationToken cancellationToken = default);
    Task<List<EmailCampaignRecipient>> GetPendingByCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
    Task AddRecipientsAsync(IEnumerable<EmailCampaignRecipient> recipients, CancellationToken cancellationToken = default);
}
