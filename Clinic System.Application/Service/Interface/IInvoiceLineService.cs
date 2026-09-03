namespace Clinic_System.Application.Service.Interface
{
    public interface IInvoiceLineService
    {
        Task<Payment> AddLinesAsync(int paymentId, List<InvoiceLineInput> lines, CancellationToken cancellationToken = default);
        Task<Payment> RemoveLineAsync(int lineId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InvoiceLine>> GetByPaymentIdAsync(int paymentId, CancellationToken cancellationToken = default);
    }

    public class InvoiceLineInput
    {
        public string Description { get; set; } = null!;
        public int? ToothNumber { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public string? UnitPriceInput { get; set; }
        public int? DentalTreatmentId { get; set; }
    }
}
