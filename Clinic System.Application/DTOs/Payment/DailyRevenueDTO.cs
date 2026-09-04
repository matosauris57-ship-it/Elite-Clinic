namespace Clinic_System.Application.DTOs.Payment
{
    public class DailyRevenueDTO
    {
        public decimal TotalRevenue { get; set; }
        public string TotalRevenueDisplay { get; set; } = string.Empty;
        public decimal CashTotal { get; set; }
        public string CashTotalDisplay { get; set; } = string.Empty;
        public decimal InstaPayTotal { get; set; }
        public string InstaPayTotalDisplay { get; set; } = string.Empty;
        public decimal CardTotal { get; set; }
        public string CardTotalDisplay { get; set; } = string.Empty;
        public decimal CashAndInstaPayTotal { get; set; }
        public string CashAndInstaPayTotalDisplay { get; set; } = string.Empty;
        public int TotalTransactions { get; set; }
        public string ReportDate { get; set; }
    }
}
