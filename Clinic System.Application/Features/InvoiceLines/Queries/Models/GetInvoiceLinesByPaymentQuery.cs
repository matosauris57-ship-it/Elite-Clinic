namespace Clinic_System.Application.Features.InvoiceLines.Queries.Models
{
    public class GetInvoiceLinesByPaymentQuery : IRequest<Response<List<InvoiceLineDTO>>>
    {
        public int PaymentId { get; set; }
    }
}
