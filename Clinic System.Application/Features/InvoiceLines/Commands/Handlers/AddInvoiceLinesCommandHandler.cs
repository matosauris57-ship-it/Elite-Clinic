namespace Clinic_System.Application.Features.InvoiceLines.Commands.Handlers
{
    public class AddInvoiceLinesCommandHandler : AppRequestHandler<AddInvoiceLinesCommand, List<InvoiceLineDTO>>
    {
        private readonly IInvoiceLineService invoiceLineService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public AddInvoiceLinesCommandHandler(
            ICurrentUserService currentUserService,
            IInvoiceLineService invoiceLineService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.invoiceLineService = invoiceLineService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<List<InvoiceLineDTO>>> Handle(AddInvoiceLinesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await invoiceLineService.AddLinesAsync(request.PaymentId, request.Lines, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);

                var lines = await invoiceLineService.GetByPaymentIdAsync(request.PaymentId, cancellationToken);
                return Success(mapper.Map<List<InvoiceLineDTO>>(lines.ToList()), "Invoice lines added.");
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
