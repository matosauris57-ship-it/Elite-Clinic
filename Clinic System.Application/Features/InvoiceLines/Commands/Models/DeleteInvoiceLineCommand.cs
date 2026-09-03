namespace Clinic_System.Application.Features.InvoiceLines.Commands.Models
{
    public class DeleteInvoiceLineCommand : IRequest<Response<List<InvoiceLineDTO>>>
    {
        public int LineId { get; set; }
    }
}
