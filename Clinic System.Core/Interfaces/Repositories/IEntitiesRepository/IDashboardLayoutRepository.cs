namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IDashboardLayoutRepository : IGenericRepository<DashboardLayout>
{
    Task<DashboardLayout?> GetClinicDefaultAsync(CancellationToken cancellationToken = default);
    Task<DashboardLayout?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
