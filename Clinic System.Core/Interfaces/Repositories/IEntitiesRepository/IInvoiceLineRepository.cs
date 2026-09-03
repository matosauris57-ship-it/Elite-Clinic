namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IInvoiceLineRepository : IGenericRepository<InvoiceLine>
    {
        Task<IEnumerable<InvoiceLine>> GetByPaymentIdAsync(int paymentId, CancellationToken cancellationToken = default);
    }
}
