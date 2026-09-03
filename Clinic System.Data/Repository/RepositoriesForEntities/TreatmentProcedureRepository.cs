namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class TreatmentProcedureRepository : GenericRepository<TreatmentProcedure>, ITreatmentProcedureRepository
    {
        public TreatmentProcedureRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<TreatmentProcedure>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
        {
            IQueryable<TreatmentProcedure> query = context.TreatmentProcedures.AsNoTracking();

            if (activeOnly)
                query = query.Where(p => p.IsActive);

            return await query
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<TreatmentProcedure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await context.TreatmentProcedures
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
        }
    }
}
