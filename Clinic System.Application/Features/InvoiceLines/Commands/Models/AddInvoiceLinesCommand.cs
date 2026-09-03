namespace Clinic_System.Application.Features.InvoiceLines.Commands.Models
{
    public class AddInvoiceLinesCommand : IRequest<Response<List<InvoiceLineDTO>>>
    {
        public int PaymentId { get; set; }
        public List<InvoiceLineInput> Lines { get; set; } = new();
    }
}
