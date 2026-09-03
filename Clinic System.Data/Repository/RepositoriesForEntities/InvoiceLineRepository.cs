namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class InvoiceLineRepository : GenericRepository<InvoiceLine>, IInvoiceLineRepository
    {
        public InvoiceLineRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<InvoiceLine>> GetByPaymentIdAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            return await context.InvoiceLines
                .AsNoTracking()
                .Where(l => l.PaymentId == paymentId)
                .ToListAsync(cancellationToken);
        }
    }
}
