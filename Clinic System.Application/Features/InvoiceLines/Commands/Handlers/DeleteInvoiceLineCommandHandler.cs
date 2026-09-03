namespace Clinic_System.Application.Features.InvoiceLines.Commands.Handlers
{
    public class DeleteInvoiceLineCommandHandler : AppRequestHandler<DeleteInvoiceLineCommand, List<InvoiceLineDTO>>
    {
        private readonly IInvoiceLineService invoiceLineService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public DeleteInvoiceLineCommandHandler(
            ICurrentUserService currentUserService,
            IInvoiceLineService invoiceLineService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.invoiceLineService = invoiceLineService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<List<InvoiceLineDTO>>> Handle(DeleteInvoiceLineCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await invoiceLineService.RemoveLineAsync(request.LineId, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);

                var lines = await invoiceLineService.GetByPaymentIdAsync(payment.Id, cancellationToken);
                return Success(mapper.Map<List<InvoiceLineDTO>>(lines.ToList()), "Línea eliminada.");
            }
            catch (NotFoundException ex)
            {
                return NotFound<List<InvoiceLineDTO>>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<List<InvoiceLineDTO>>(ex.Message);
            }
        }
    }
}
