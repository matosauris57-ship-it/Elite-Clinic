namespace DentalCare.Admin.Models;

public class DashboardAlert
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public string Time { get; set; } = string.Empty;
    public string? ActionLabel { get; set; }
    public string? ActionHref { get; set; }
}

public class DashboardData
{
    public string? ApiConnectionError { get; set; }
    public DateTime Today { get; set; } = DateTime.Today;
    public int AppointmentsToday { get; set; }
    public int WaitingCount { get; set; }
    public decimal DailyRevenue { get; set; }
    public int PendingTreatmentsCount { get; set; }
    public int OverdueCount { get; set; }
    public int CriticalAlerts { get; set; }
    public bool BillingRestricted { get; set; }
    public bool TreatmentsRestricted { get; set; }
    public List<AppointmentAgendaItem> TodayAppointments { get; set; } = [];
    public List<DashboardWaitingItem> WaitingPatients { get; set; } = [];
    public List<PaymentListItem> TodayCollections { get; set; } = [];
    public List<DentalTreatmentListItem> PendingTreatments { get; set; } = [];
    public List<PaymentListItem> UpcomingDues { get; set; } = [];
    public List<decimal> MonthlyIncome { get; set; } = [];
    public decimal YearToDateRevenue { get; set; }
    public List<decimal> SparklineRevenue { get; set; } = [];
    public List<DashboardAlert> Alerts { get; set; } = [];
    public AppointmentStats? TodayStats { get; set; }
    public PatientDashboardStats? PatientStats { get; set; }
    public string? PatientsError { get; set; }
    public List<int> WeekAppointmentCounts { get; set; } = [];
    public List<string> WeekDayLabels { get; set; } = [];
    public int CompletedTreatmentsCount { get; set; }
    public List<TreatmentMixSlice> TreatmentMix { get; set; } = [];
    public decimal RevenueToday { get; set; }
    public decimal RevenueWeek { get; set; }
    public decimal RevenueMonth { get; set; }
    public decimal RevenueYear { get; set; }
    public decimal CashToday { get; set; }
    public decimal CardToday { get; set; }
    public decimal InstaPayToday { get; set; }
    public List<RecentClinicalActivityItem> RecentActivity { get; set; } = [];
    public string? ActivityError { get; set; }
    public int PeriodontalIncompleteCount { get; set; }
    public string? PeriodontalError { get; set; }
    public bool PatientsRestricted { get; set; }
    public bool HistoryRestricted { get; set; }
    public bool PeriodontalRestricted { get; set; }
}

public class DashboardWaitingItem
{
    public AppointmentAgendaItem Appointment { get; set; } = new();
    public int WaitMinutes { get; set; }
}
