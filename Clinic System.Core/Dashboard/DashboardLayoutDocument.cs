namespace Clinic_System.Core.Dashboard;

public class DashboardLayoutDocument
{
    public int Version { get; set; } = 1;
    public List<DashboardWidgetPlacement> Items { get; set; } = [];
}

public class DashboardWidgetPlacement
{
    public string Id { get; set; } = string.Empty;
    public string WidgetKey { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; } = 3;
    public int H { get; set; } = 2;
    public Dictionary<string, string> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class DashboardWidgetSize
{
    public int W { get; set; }
    public int H { get; set; }
}

public sealed class DashboardWidgetDefinition
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Permission { get; init; }
    public bool Required { get; init; }
    public int DefaultX { get; init; }
    public int DefaultY { get; init; }
    public int DefaultW { get; init; }
    public int DefaultH { get; init; }
    public int MinW { get; init; } = 2;
    public int MinH { get; init; } = 2;
    public int MaxW { get; init; } = 12;
    public int MaxH { get; init; } = 8;
    public DashboardWidgetSize Small { get; init; } = new() { W = 3, H = 2 };
    public DashboardWidgetSize Medium { get; init; } = new() { W = 6, H = 3 };
    public DashboardWidgetSize Large { get; init; } = new() { W = 12, H = 5 };
}

public static class DashboardWidgetKeys
{
    public const string AppointmentsTodayKpi = "appointments-today";
    public const string WaitingCountKpi = "waiting-count";
    public const string DailyRevenueKpi = "daily-revenue";
    public const string PendingTreatmentsKpi = "pending-treatments-count";
    public const string TodayAppointments = "today-appointments";
    public const string DoctorsDailyAgenda = "doctors-daily-agenda";
    public const string WaitingQueue = "waiting-queue";
    public const string DailyCollections = "daily-collections";
    public const string MonthlyIncomeChart = "monthly-income";
    public const string SmartAlerts = "smart-alerts";
    public const string PendingTreatments = "pending-treatments";
    public const string UpcomingDues = "upcoming-dues";
    public const string PatientsRegistered = "patients-registered";
    public const string PatientsNew = "patients-new";
    public const string UpcomingAppointments = "upcoming-appointments";
    public const string AppointmentsCompleted = "appointments-completed";
    public const string AppointmentsCancelled = "appointments-cancelled";
    public const string AppointmentsNoShow = "appointments-noshow";
    public const string AppointmentsWeekChart = "appointments-week";
    public const string TreatmentsCompleted = "treatments-completed";
    public const string TreatmentMix = "treatment-mix";
    public const string RevenueSummary = "revenue-summary";
    public const string PaymentMethods = "payment-methods";
    public const string RecentActivity = "recent-activity";
    public const string PeriodontalIncomplete = "periodontal-incomplete";
    public const string SystemStatus = "system-status";
}
