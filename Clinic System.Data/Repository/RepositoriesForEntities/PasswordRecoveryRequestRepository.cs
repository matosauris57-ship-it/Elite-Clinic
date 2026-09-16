using Clinic_System.Core.Entities;
using Clinic_System.Core.Enums;
using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class PasswordRecoveryRequestRepository : GenericRepository<PasswordRecoveryRequest>, IPasswordRecoveryRequestRepository
{
    public PasswordRecoveryRequestRepository(AppDbContext context) : base(context)
    {
    }

    public Task<PasswordRecoveryRequest?> FindRecentPendingAsync(string normalizedIdentifier, DateTime since, CancellationToken cancellationToken = default)
    {
        return context.PasswordRecoveryRequests
            .Where(x => x.Status == PasswordRecoveryStatus.Pending
                && x.NormalizedIdentifier == normalizedIdentifier
                && x.RequestedAt >= since)
            .OrderByDescending(x => x.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountPendingAsync(CancellationToken cancellationToken = default) =>
        context.PasswordRecoveryRequests.CountAsync(x => x.Status == PasswordRecoveryStatus.Pending, cancellationToken);

    public async Task<List<PasswordRecoveryRequest>> ListAsync(PasswordRecoveryStatus? status, CancellationToken cancellationToken = default)
    {
        var query = context.PasswordRecoveryRequests.AsNoTracking().AsQueryable();
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query
            .OrderByDescending(x => x.RequestedAt)
            .Take(200)
            .ToListAsync(cancellationToken);
    }
}
