namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class TreatmentPlanRepository : GenericRepository<TreatmentPlan>, ITreatmentPlanRepository
    {
        public TreatmentPlanRepository(AppDbContext context) : base(context) { }

        public async Task<TreatmentPlan?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.TreatmentPlans
                .Include(p => p.Items)
                    .ThenInclude(i => i.DentalTreatment)
                .Include(p => p.Patient)
                .Include(p => p.InvoicePayment)
                    .ThenInclude(pay => pay!.InvoiceLines)
                .Include(p => p.InvoicePayment)
                    .ThenInclude(pay => pay!.Receipts)
                .Include(p => p.Invoices)
                    .ThenInclude(i => i.InvoiceLines)
                .Include(p => p.Invoices)
                    .ThenInclude(i => i.Receipts)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<TreatmentPlan>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await context.TreatmentPlans
                .AsNoTracking()
                .Include(p => p.Items)
                    .ThenInclude(i => i.DentalTreatment)
                .Include(p => p.Patient)
                .Include(p => p.InvoicePayment)
                    .ThenInclude(pay => pay!.InvoiceLines)
                .Include(p => p.InvoicePayment)
                    .ThenInclude(pay => pay!.Receipts)
                .Include(p => p.Invoices)
                    .ThenInclude(i => i.InvoiceLines)
                .Include(p => p.Invoices)
                    .ThenInclude(i => i.Receipts)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<PlanItem?> GetItemWithPlanAsync(int itemId, CancellationToken cancellationToken = default)
        {
            return await context.Set<PlanItem>()
                .Include(i => i.TreatmentPlan)
                    .ThenInclude(p => p.Patient)
                .Include(i => i.DentalTreatment)
                .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        }
    }
}
