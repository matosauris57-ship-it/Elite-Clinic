namespace Clinic_System.Application.Features.Payment.Queries.Handlers
{
    public class GetDailyRevenueQueryHandler : ResponseHandler, IRequestHandler<GetDailyRevenueQuery, Response<DailyRevenueDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetDailyRevenueQueryHandler> _logger;

        public GetDailyRevenueQueryHandler(IUnitOfWork unitOfWork, ILogger<GetDailyRevenueQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DailyRevenueDTO>> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating financial report for date: {Date}", request.Date ?? DateTime.Today);

            try
            {
                var targetDate = request.Date ?? DateTime.Today;

                var (total, cash, insta, card, count) = await _unitOfWork.PaymentsRepository.GetDailyTotalsAsync(targetDate);

                var cashAndInsta = Money.Normalize(cash + insta);
                var response = new DailyRevenueDTO
                {
                    TotalRevenue = Money.Normalize(total),
                    TotalRevenueDisplay = Money.Format(total),
                    CashTotal = Money.Normalize(cash),
                    CashTotalDisplay = Money.Format(cash),
                    InstaPayTotal = Money.Normalize(insta),
                    InstaPayTotalDisplay = Money.Format(insta),
                    CardTotal = Money.Normalize(card),
                    CardTotalDisplay = Money.Format(card),
                    CashAndInstaPayTotal = cashAndInsta,
                    CashAndInstaPayTotalDisplay = Money.Format(cashAndInsta),
                    TotalTransactions = count,
                    ReportDate = targetDate.ToString("yyyy-MM-dd")
                };

                _logger.LogInformation("Daily revenue report generated successfully for date: {Date}", response.ReportDate);
                return Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating daily revenue report.");
                return BadRequest<DailyRevenueDTO>("An error occurred while calculating the revenue.");
            }
        }
    }
}
