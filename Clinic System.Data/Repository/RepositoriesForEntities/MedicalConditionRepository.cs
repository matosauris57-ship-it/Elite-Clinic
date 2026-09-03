namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class MedicalConditionRepository : GenericRepository<MedicalCondition>, IMedicalConditionRepository
    {
        public MedicalConditionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<MedicalCondition>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
        {
            IQueryable<MedicalCondition> query = context.MedicalConditions.AsNoTracking();

            if (activeOnly)
                query = query.Where(c => c.IsActive);

            return await query
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<MedicalCondition?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalized = name.Trim().ToLowerInvariant();
            return await context.MedicalConditions
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized, cancellationToken);
        }
    }
}
