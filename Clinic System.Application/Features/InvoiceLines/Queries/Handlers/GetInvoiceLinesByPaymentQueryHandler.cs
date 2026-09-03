namespace Clinic_System.Application.Features.InvoiceLines.Queries.Handlers
{
    public class GetInvoiceLinesByPaymentQueryHandler : AppRequestHandler<GetInvoiceLinesByPaymentQuery, List<InvoiceLineDTO>>
    {
        private readonly IInvoiceLineService invoiceLineService;
        private readonly IMapper mapper;

        public GetInvoiceLinesByPaymentQueryHandler(
            ICurrentUserService currentUserService,
            IInvoiceLineService invoiceLineService,
            IMapper mapper) : base(currentUserService)
        {
            this.invoiceLineService = invoiceLineService;
            this.mapper = mapper;
        }

        public override async Task<Response<List<InvoiceLineDTO>>> Handle(GetInvoiceLinesByPaymentQuery request, CancellationToken cancellationToken)
        {
            var lines = await invoiceLineService.GetByPaymentIdAsync(request.PaymentId, cancellationToken);
            return Success(mapper.Map<List<InvoiceLineDTO>>(lines.ToList()));
        }
    }
}
