namespace Clinic_System.Application.Features.Payment.Commands.Handlers
{
    public class ApplyInvoiceDiscountCommandHandler : AppRequestHandler<ApplyInvoiceDiscountCommand, PaymentDetailsDTO>
    {
        private readonly IPaymentService paymentService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ApplyInvoiceDiscountCommandHandler(
            ICurrentUserService currentUserService,
            IPaymentService paymentService,
            IUnitOfWork unitOfWork,
            IMapper mapper) : base(currentUserService)
        {
            this.paymentService = paymentService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public override async Task<Response<PaymentDetailsDTO>> Handle(ApplyInvoiceDiscountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                decimal discount;
                if (!string.IsNullOrWhiteSpace(request.DiscountAmountInput))
                {
                    if (!Money.TryParse(request.DiscountAmountInput, out discount))
                        throw new InvalidOperationException("El descuento no es válido.");
                }
                else
                {
                    discount = Money.Normalize(request.DiscountAmount);
                }

                await paymentService.ApplyDiscountAsync(request.PaymentId, discount, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);

                var payment = await unitOfWork.PaymentsRepository.GetPaymentDetailsByIdAsync(request.PaymentId);
                return Success(mapper.Map<PaymentDetailsDTO>(payment), "Descuento aplicado a la factura.");
            }
            catch (NotFoundException ex)
            {
                return NotFound<PaymentDetailsDTO>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<PaymentDetailsDTO>(ex.Message);
            }
        }
    }
}
