namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class DoctorProcedurePriceRepository : GenericRepository<DoctorProcedurePrice>, IDoctorProcedurePriceRepository
    {
        public DoctorProcedurePriceRepository(AppDbContext context) : base(context) { }

        public async Task<IReadOnlyList<DoctorProcedurePrice>> GetByProcedureIdAsync(int procedureId, CancellationToken cancellationToken = default)
        {
            return await context.DoctorProcedurePrices
                .Where(p => p.TreatmentProcedureId == procedureId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<DoctorProcedurePrice>> GetByProcedureIdsAsync(IEnumerable<int> procedureIds, CancellationToken cancellationToken = default)
        {
            var ids = procedureIds.Distinct().ToList();
            if (ids.Count == 0)
                return [];

            return await context.DoctorProcedurePrices
                .AsNoTracking()
                .Where(p => ids.Contains(p.TreatmentProcedureId))
                .ToListAsync(cancellationToken);
        }

        public Task<DoctorProcedurePrice?> GetAsync(int doctorId, int procedureId, CancellationToken cancellationToken = default)
        {
            return context.DoctorProcedurePrices
                .FirstOrDefaultAsync(p => p.DoctorId == doctorId && p.TreatmentProcedureId == procedureId, cancellationToken);
        }

        public void RemoveRange(IEnumerable<DoctorProcedurePrice> prices)
        {
            context.DoctorProcedurePrices.RemoveRange(prices);
        }
    }
}
