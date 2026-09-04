namespace Clinic_System.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    [Authorize]
    public class PaymentController : AppControllerBase
    {
        public PaymentController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("list")]
        [Authorize(Policy = "facturacion.view")]
        public async Task<IActionResult> GetAllPaymentsAsync([FromQuery] GetPaymentsListQuery query, CancellationToken cancellationToken)
        {
            var payments = await mediator.Send(query, cancellationToken);
            return NewResult(payments);
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "facturacion.view")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var response = await mediator.Send(new GetPaymentDetailsByIdQuery { Id = id });
            return NewResult(response);
        }

        [HttpGet("daily-revenue")]
        [Authorize(Policy = "facturacion.view")]
        public async Task<IActionResult> GetDailyRevenueAsync([FromQuery] GetDailyRevenueQuery query, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(query, cancellationToken);
            return NewResult(response);
        }

        [HttpGet("doctor-revenue")]
        [Authorize(Policy = "facturacion.view")]
        public async Task<IActionResult> GetDoctorRevenueAsync([FromQuery] GetDoctorRevenueQuery query, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(query, cancellationToken);
            return NewResult(response);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "facturacion.edit")]
        public async Task<IActionResult> UpdatePaymentAsync([FromRoute] int id, [FromBody] UpdatePaymentCommand command, CancellationToken cancellationToken)
        {
            if (id != command.PaymentId)
            {
                return BadRequest("Payment ID mismatch between route and body.");
            }

            var result = await mediator.Send(command, cancellationToken);
            return NewResult(result);
        }

        [HttpPut("{id:int}/collect")]
        [Authorize(Policy = "facturacion.edit")]
        public async Task<IActionResult> CollectPayment(int id, [FromBody] CollectPaymentCommand? command)
        {
            command ??= new CollectPaymentCommand();
            command.PaymentId = id;
            if (!Enum.IsDefined(command.PaymentMethod) || (int)command.PaymentMethod == 0)
                command.PaymentMethod = PaymentMethod.Cash;

            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPut("{id:int}/refund")]
        [Authorize(Policy = "facturacion.edit")]
        public async Task<IActionResult> RefundPayment(int id, [FromBody] RefundPaymentCommand command)
        {
            command.PaymentId = id;
            var response = await mediator.Send(command);
            return NewResult(response);
        }

        [HttpPut("{id:int}/cancel")]
        [Authorize(Policy = "facturacion.edit")]
        public async Task<IActionResult> CancelPayment(int id, [FromBody] CancelPaymentCommand command)
        {
            command.PaymentId = id;
            var response = await mediator.Send(command);
            return NewResult(response);
        }
    }
}
