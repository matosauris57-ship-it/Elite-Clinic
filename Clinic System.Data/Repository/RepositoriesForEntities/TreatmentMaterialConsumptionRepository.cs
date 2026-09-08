namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class TreatmentMaterialConsumptionRepository
        : GenericRepository<TreatmentMaterialConsumption>, ITreatmentMaterialConsumptionRepository
    {
        public TreatmentMaterialConsumptionRepository(AppDbContext context) : base(context) { }

        public Task<TreatmentMaterialConsumption?> GetByTreatmentIdAsync(
            int dentalTreatmentId, CancellationToken cancellationToken = default)
        {
            return context.TreatmentMaterialConsumptions
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.DentalTreatmentId == dentalTreatmentId, cancellationToken);
        }

        public Task<TreatmentMaterialConsumption?> GetByTreatmentIdWithLinesAsync(
            int dentalTreatmentId, CancellationToken cancellationToken = default)
        {
            return context.TreatmentMaterialConsumptions
                .AsNoTracking()
                .Include(c => c.Lines)
                    .ThenInclude(l => l.InventoryItem)
                .FirstOrDefaultAsync(c => c.DentalTreatmentId == dentalTreatmentId, cancellationToken);
        }

        public Task<TreatmentMaterialConsumption?> GetByTreatmentIdForUpdateAsync(
            int dentalTreatmentId, CancellationToken cancellationToken = default)
        {
            return context.TreatmentMaterialConsumptions
                .Include(c => c.Lines)
                .FirstOrDefaultAsync(c => c.DentalTreatmentId == dentalTreatmentId, cancellationToken);
        }
    }
}
