namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class ProcedureMaterialRepository : GenericRepository<ProcedureMaterial>, IProcedureMaterialRepository
    {
        public ProcedureMaterialRepository(AppDbContext context) : base(context) { }

        public Task<List<ProcedureMaterial>> GetByProcedureIdAsync(
            int treatmentProcedureId, CancellationToken cancellationToken = default)
        {
            return context.ProcedureMaterials
                .AsNoTracking()
                .Where(p => p.TreatmentProcedureId == treatmentProcedureId)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Id)
                .ToListAsync(cancellationToken);
        }

        public Task<List<ProcedureMaterial>> GetByProcedureIdWithItemsAsync(
            int treatmentProcedureId, CancellationToken cancellationToken = default)
        {
            return context.ProcedureMaterials
                .AsNoTracking()
                .Include(p => p.InventoryItem)
                .Where(p => p.TreatmentProcedureId == treatmentProcedureId)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
