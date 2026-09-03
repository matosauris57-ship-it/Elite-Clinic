namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class EmailCampaignRepository : GenericRepository<EmailCampaign>, IEmailCampaignRepository
{
    public EmailCampaignRepository(AppDbContext context) : base(context)
    {
    }

    public Task<List<EmailCampaign>> ListAsync(CancellationToken cancellationToken = default) =>
        context.EmailCampaigns
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<EmailCampaign?> GetWithRecipientsAsync(int id, CancellationToken cancellationToken = default) =>
        context.EmailCampaigns
            .AsNoTracking()
            .Include(x => x.Recipients)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<EmailCampaign?> GetTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        context.EmailCampaigns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<EmailCampaignRecipient>> TakePendingAsync(int take, CancellationToken cancellationToken = default) =>
        context.Set<EmailCampaignRecipient>()
            .Include(x => x.EmailCampaign)
            .Include(x => x.Patient)
            .Where(x => x.Status == EmailCampaignRecipientStatus.Pending
                && x.EmailCampaign.Status == EmailCampaignStatus.Running)
            .OrderBy(x => x.EmailCampaignId)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<List<EmailCampaignRecipient>> GetPendingByCampaignAsync(int campaignId, CancellationToken cancellationToken = default) =>
        context.EmailCampaignRecipients
            .Where(x => x.EmailCampaignId == campaignId && x.Status == EmailCampaignRecipientStatus.Pending)
            .ToListAsync(cancellationToken);

    public async Task AddRecipientsAsync(IEnumerable<EmailCampaignRecipient> recipients, CancellationToken cancellationToken = default)
    {
        await context.Set<EmailCampaignRecipient>().AddRangeAsync(recipients, cancellationToken);
    }
}
