namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IDentalHistoryRepository : IGenericRepository<DentalHistory>
    {
        Task<DentalHistory?> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    }
}
