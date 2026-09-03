namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class TreatmentPlanRepository : GenericRepository<TreatmentPlan>, ITreatmentPlanRepository
    {
        public TreatmentPlanRepository(AppDbContext context) : base(context) { }

        public async Task<TreatmentPlan?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.TreatmentPlans
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<TreatmentPlan>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await context.TreatmentPlans
                .AsNoTracking()
                .Include(p => p.Items)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
