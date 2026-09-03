namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class DentalHistoryRepository : GenericRepository<DentalHistory>, IDentalHistoryRepository
    {
        public DentalHistoryRepository(AppDbContext context) : base(context) { }

        public async Task<DentalHistory?> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await context.DentalHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.PatientId == patientId, cancellationToken);
        }
    }
}
