namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class DashboardLayoutRepository : GenericRepository<DashboardLayout>, IDashboardLayoutRepository
{
    public DashboardLayoutRepository(AppDbContext context) : base(context)
    {
    }

    public Task<DashboardLayout?> GetClinicDefaultAsync(CancellationToken cancellationToken = default) =>
        context.DashboardLayouts
            .FirstOrDefaultAsync(x => x.Scope == DashboardLayoutScopes.Clinic && x.UserId == null, cancellationToken);

    public Task<DashboardLayout?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        context.DashboardLayouts
            .FirstOrDefaultAsync(x => x.Scope == DashboardLayoutScopes.User && x.UserId == userId, cancellationToken);
}
