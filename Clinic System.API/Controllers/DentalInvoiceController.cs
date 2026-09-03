namespace Clinic_System.API.Controllers
{
    [Route("api/dental/invoices")]
    [ApiController]
    [Authorize]
    public class DentalInvoiceController : AppControllerBase
    {
        public DentalInvoiceController(IMediator mediator) : base(mediator) { }

        [HttpGet("payment/{paymentId:int}")]
        [Authorize(Policy = "facturacion.view")]
        public async Task<IActionResult> GetByPayment(int paymentId)
        {
            var response = await mediator.Send(new GetInvoiceLinesByPaymentQuery { PaymentId = paymentId });
            return NewResult(response);
        }

        [HttpPost("lines")]
        [Authorize(Policy = "facturacion.edit")]
        public async Task<IActionResult> AddLines([FromBody] AddInvoiceLinesCommand command)
        {
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpDelete("lines/{id:int}")]
        [Authorize(Policy = "facturacion.edit")]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var response = await mediator.Send(new DeleteInvoiceLineCommand { LineId = id });
            return NewResult(response);
        }
    }
}
