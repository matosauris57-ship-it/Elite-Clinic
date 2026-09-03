namespace Clinic_System.Application.Features.Payment.Commands.Handlers
{
    public class CollectPaymentCommandHandler : AppRequestHandler<CollectPaymentCommand, PaymentDetailsDTO>
    {
        private readonly IPaymentService paymentService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CollectPaymentCommandHandler(
            ICurrentUserService currentUserService,
            IPaymentService paymentService,
            IUnitOfWork unitOfWork,
            IMapper mapper) : base(currentUserService)
        {
            this.paymentService = paymentService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public override async Task<Response<PaymentDetailsDTO>> Handle(CollectPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var collected = await paymentService.CollectAsync(
                    request.PaymentId,
                    request.PaymentMethod,
                    request.Notes,
                    Money.Resolve(request.AmountInput, request.Amount),
                    cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);

                var payment = await unitOfWork.PaymentsRepository.GetPaymentDetailsByIdAsync(request.PaymentId);
                var message = collected.PaymentStatus == PaymentStatus.Paid
                    ? "Factura saldada."
                    : "Abono registrado.";
                return Success(mapper.Map<PaymentDetailsDTO>(payment), message);
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
