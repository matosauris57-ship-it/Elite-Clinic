namespace Clinic_System.Application.Features.Payment.Commands.Handlers
{
    public class RefundPaymentCommandHandler : AppRequestHandler<RefundPaymentCommand, PaymentDetailsDTO>
    {
        private readonly IPaymentService paymentService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public RefundPaymentCommandHandler(
            ICurrentUserService currentUserService,
            IPaymentService paymentService,
            IUnitOfWork unitOfWork,
            IMapper mapper) : base(currentUserService)
        {
            this.paymentService = paymentService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public override async Task<Response<PaymentDetailsDTO>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await paymentService.RefundAsync(request.PaymentId, request.Reason, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);

                var payment = await unitOfWork.PaymentsRepository.GetPaymentDetailsByIdAsync(request.PaymentId);
                return Success(mapper.Map<PaymentDetailsDTO>(payment), "Pago reembolsado.");
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
