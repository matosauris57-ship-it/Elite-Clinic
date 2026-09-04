using System.Globalization;
using Clinic_System.Core.Dashboard;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class DashboardService
{
    private static readonly CultureInfo EsDo = CultureInfo.GetCultureInfo("es-DO");

    private readonly AgendaMaintenanceService _agenda;
    private readonly BillingMaintenanceService _billing;
    private readonly ClinicalTreatmentMaintenanceService _treatments;
    private readonly DashboardLayoutApiService _layoutApi;
    private readonly ApiSettings _apiSettings;

    public DashboardService(
        AgendaMaintenanceService agenda,
        BillingMaintenanceService billing,
        ClinicalTreatmentMaintenanceService treatments,
        DashboardLayoutApiService layoutApi,
        IOptions<ApiSettings> apiSettings)
    {
        _agenda = agenda;
        _billing = billing;
        _treatments = treatments;
        _layoutApi = layoutApi;
        _apiSettings = apiSettings.Value;
    }

    public Task<DashboardData> GetDashboardDataAsync() =>
        GetDashboardDataAsync(DashboardWidgetCatalog.All.Select(d => d.Key));

    public async Task<DashboardData> GetDashboardDataAsync(IEnumerable<string> widgetKeys)
    {
        var keys = widgetKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.Now;
        var today = now.Date;
        var yearStart = new DateTime(today.Year, 1, 1);
        var weekStart = today.AddDays(-((int)today.DayOfWeek + 6) % 7);

        var needAgenda = Needs(keys,
            DashboardWidgetKeys.AppointmentsTodayKpi,
            DashboardWidgetKeys.WaitingCountKpi,
            DashboardWidgetKeys.TodayAppointments,
            DashboardWidgetKeys.DoctorsDailyAgenda,
            DashboardWidgetKeys.WaitingQueue,
            DashboardWidgetKeys.UpcomingAppointments,
            DashboardWidgetKeys.SmartAlerts);
        var needWeek = Needs(keys, DashboardWidgetKeys.AppointmentsWeekChart);
        var needStats = Needs(keys,
            DashboardWidgetKeys.AppointmentsCompleted,
            DashboardWidgetKeys.AppointmentsCancelled,
            DashboardWidgetKeys.AppointmentsNoShow,
            DashboardWidgetKeys.SmartAlerts);
        var needBilling = Needs(keys,
            DashboardWidgetKeys.DailyRevenueKpi,
            DashboardWidgetKeys.DailyCollections,
            DashboardWidgetKeys.MonthlyIncomeChart,
            DashboardWidgetKeys.UpcomingDues,
            DashboardWidgetKeys.PaymentMethods,
            DashboardWidgetKeys.RevenueSummary,
            DashboardWidgetKeys.SmartAlerts);
        var needTreatments = Needs(keys,
            DashboardWidgetKeys.PendingTreatmentsKpi,
            DashboardWidgetKeys.PendingTreatments,
            DashboardWidgetKeys.TreatmentsCompleted,
            DashboardWidgetKeys.TreatmentMix,
            DashboardWidgetKeys.SmartAlerts);
        var needPatients = Needs(keys, DashboardWidgetKeys.PatientsRegistered, DashboardWidgetKeys.PatientsNew);
        var needActivity = Needs(keys, DashboardWidgetKeys.RecentActivity);
        var needPeriodontal = Needs(keys, DashboardWidgetKeys.PeriodontalIncomplete);

        var agendaTask = needAgenda ? _agenda.GetAgendaAsync(today) : CompletedAgenda();
        var weekAgendaTask = needWeek ? _agenda.GetAgendaAsync(weekStart, endDate: weekStart.AddDays(6)) : CompletedAgenda();
        var statsTask = needStats ? _agenda.GetDayStatsAsync(today) : Task.FromResult<(AppointmentStats?, string?)>((null, null));
        var dailyRevenueTask = needBilling ? _billing.GetDailyRevenueAsync(today) : Task.FromResult<(DailyRevenue?, string?)>((null, null));
        var todayPaymentsTask = needBilling
            ? _billing.GetPaymentsAsync(new BillingPaymentFilters { FromDate = today, ToDate = today, PageNumber = 1, PageSize = 100 })
            : Task.FromResult((new PagedResult<PaymentListItem>(), (string?)null));
        var yearPaymentsTask = needBilling
            ? _billing.GetPaymentsAsync(new BillingPaymentFilters { FromDate = yearStart, ToDate = today, PageNumber = 1, PageSize = 100 })
            : Task.FromResult((new PagedResult<PaymentListItem>(), (string?)null));
        var pendingPaymentsTask = needBilling
            ? _billing.GetPaymentsAsync(new BillingPaymentFilters { Status = "Pending", PageNumber = 1, PageSize = 100 })
            : Task.FromResult((new PagedResult<PaymentListItem>(), (string?)null));
        var partialPaymentsTask = needBilling
            ? _billing.GetPaymentsAsync(new BillingPaymentFilters { Status = "PartiallyPaid", PageNumber = 1, PageSize = 100 })
            : Task.FromResult((new PagedResult<PaymentListItem>(), (string?)null));
        var treatmentsTask = needTreatments ? _treatments.GetTreatmentsAsync(null, null) : Task.FromResult((new List<DentalTreatmentListItem>(), (string?)null));
        var patientsTask = needPatients ? _layoutApi.GetPatientStatsAsync() : Task.FromResult<(PatientDashboardStats?, string?)>((null, null));
        var activityTask = needActivity ? _layoutApi.GetRecentActivityAsync("7d", 10) : Task.FromResult<(List<RecentClinicalActivityItem>?, string?)>(([], null));
        var periodontalTask = needPeriodontal ? _layoutApi.GetPeriodontalIncompleteAsync() : Task.FromResult<(PeriodontalIncompleteStats?, string?)>((null, null));

        await Task.WhenAll(
            agendaTask,
            weekAgendaTask,
            statsTask,
            dailyRevenueTask,
            todayPaymentsTask,
            yearPaymentsTask,
            pendingPaymentsTask,
            partialPaymentsTask,
            treatmentsTask,
            patientsTask,
            activityTask,
            periodontalTask);

        var (agenda, agendaError) = await agendaTask;
        var (weekAgenda, _) = await weekAgendaTask;
        var (stats, _) = await statsTask;
        var (dailyRevenue, dailyRevenueError) = await dailyRevenueTask;
        var (todayPage, todayPayError) = await todayPaymentsTask;
        var (yearPage, yearPayError) = await yearPaymentsTask;
        var (pendingPage, pendingPayError) = await pendingPaymentsTask;
        var (partialPage, partialPayError) = await partialPaymentsTask;
        var (treatmentItems, treatmentError) = await treatmentsTask;
        var (patientStats, patientsError) = await patientsTask;
        var (activity, activityError) = await activityTask;
        var (periodontal, periodontalError) = await periodontalTask;

        if (IsConnectionError(agendaError) && agenda.Count == 0)
        {
            return new DashboardData
            {
                ApiConnectionError = ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl),
                Today = today
            };
        }

        var billingRestricted = IsRestricted(dailyRevenueError)
            || IsRestricted(todayPayError)
            || IsRestricted(yearPayError)
            || IsRestricted(pendingPayError)
            || IsRestricted(partialPayError);

        var treatmentsRestricted = !string.IsNullOrWhiteSpace(treatmentError) && !IsConnectionError(treatmentError);

        var activeAppointments = agenda
            .Where(a => !IsTerminalStatus(a.Status))
            .OrderBy(a => a.ParsedDate ?? DateTime.MaxValue)
            .ToList();

        var waiting = activeAppointments
            .Where(a => a.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
            .Select(a => new DashboardWaitingItem
            {
                Appointment = a,
                WaitMinutes = a.ParsedDate is DateTime start && start <= now
                    ? Math.Max(0, (int)(now - start).TotalMinutes)
                    : -1
            })
            .Where(w => w.WaitMinutes >= 0)
            .OrderByDescending(w => w.WaitMinutes)
            .ToList();

        var todayItems = todayPage.Items?.ToList() ?? [];
        var collections = todayItems.Where(IsCollected).OrderByDescending(p => p.AmountCollected).ToList();

        var pendingTreatments = treatmentsRestricted
            ? []
            : treatmentItems
                .Where(t => t.Status is "Planned" or "InProgress")
                .OrderBy(t => t.Status == "InProgress" ? 0 : 1)
                .ThenByDescending(t => t.CreatedAt)
                .Take(8)
                .ToList();

        var dues = billingRestricted
            ? []
            : (pendingPage.Items ?? [])
                .Concat(partialPage.Items ?? [])
                .Where(p => p.Balance > 0)
                .GroupBy(p => p.PaymentId)
                .Select(g => g.First())
                .OrderBy(p => ParseDate(p.PaymentDate) ?? ParseDate(p.AppointmentDate) ?? DateTime.MaxValue)
                .Take(8)
                .ToList();

        var yearItems = yearPage.Items?.ToList() ?? [];
        var monthly = new decimal[12];
        foreach (var payment in yearItems.Where(IsCollected))
        {
            var date = ParseDate(payment.PaymentDate) ?? ParseDate(payment.AppointmentDate);
            if (date is null || date.Value.Year != today.Year)
                continue;
            monthly[date.Value.Month - 1] += payment.AmountCollected > 0 ? payment.AmountCollected : payment.Amount;
        }

        var sparkline = new List<decimal>();
        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            sparkline.Add(yearItems.Where(p => IsCollected(p) && SameDay(p, day)).Sum(CollectedAmount));
        }

        var yearTotal = monthly.Sum();
        var dailyTotal = dailyRevenue?.TotalRevenue ?? collections.Sum(CollectedAmount);

        var data = new DashboardData
        {
            Today = today,
            AppointmentsToday = activeAppointments.Count,
            WaitingCount = waiting.Count,
            DailyRevenue = dailyTotal,
            PendingTreatmentsCount = treatmentsRestricted
                ? 0
                : treatmentItems.Count(t => t.Status is "Planned" or "InProgress"),
            OverdueCount = dues.Count,
            BillingRestricted = billingRestricted,
            TreatmentsRestricted = treatmentsRestricted,
            TodayAppointments = activeAppointments,
            WaitingPatients = waiting,
            TodayCollections = collections,
            PendingTreatments = pendingTreatments,
            UpcomingDues = dues,
            MonthlyIncome = monthly.ToList(),
            YearToDateRevenue = yearTotal,
            SparklineRevenue = sparkline,
            TodayStats = stats,
            PatientStats = patientStats,
            PatientsError = patientsError,
            PatientsRestricted = IsRestricted(patientsError),
            WeekAppointmentCounts = BuildWeekCounts(weekAgenda, weekStart),
            WeekDayLabels = ["Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom"],
            CompletedTreatmentsCount = treatmentsRestricted ? 0 : treatmentItems.Count(t => t.Status == "Completed"),
            TreatmentMix = BuildMix(treatmentItems, treatmentsRestricted),
            RevenueToday = dailyTotal,
            RevenueWeek = SumCollected(yearItems, weekStart, today),
            RevenueMonth = monthly[today.Month - 1],
            RevenueYear = yearTotal,
            CashToday = dailyRevenue?.CashTotal ?? collections.Where(p => p.PaymentMethod == "Cash").Sum(CollectedAmount),
            CardToday = dailyRevenue?.CardTotal ?? collections.Where(p => p.PaymentMethod == "Card").Sum(CollectedAmount),
            InstaPayToday = dailyRevenue?.InstaPayTotal ?? collections.Where(p => p.PaymentMethod == "InstaPay").Sum(CollectedAmount),
            RecentActivity = activity ?? [],
            ActivityError = activityError,
            HistoryRestricted = IsRestricted(activityError),
            PeriodontalIncompleteCount = periodontal?.IncompleteExams ?? 0,
            PeriodontalError = periodontalError,
            PeriodontalRestricted = IsRestricted(periodontalError)
        };

        data.Alerts = BuildAlerts(data, stats);
        data.CriticalAlerts = data.Alerts.Count(a => a.Severity is "critical" or "warning");
        return data;
    }

    private static List<DashboardAlert> BuildAlerts(DashboardData data, AppointmentStats? stats)
    {
        var alerts = new List<DashboardAlert>();
        var now = DateTime.Now.ToString("HH:mm", EsDo);

        if (stats?.NoShow > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Title = "Pacientes ausentes",
                Description = $"{stats.NoShow} cita(s) marcadas como no asistió hoy.",
                Severity = "critical",
                Time = now,
                ActionLabel = "Ver agenda",
                ActionHref = "/agenda"
            });
        }

        if (stats?.Pending > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Title = "Citas por confirmar",
                Description = $"{stats.Pending} cita(s) pendientes de confirmación.",
                Severity = "warning",
                Time = now,
                ActionLabel = "Confirmar",
                ActionHref = "/agenda"
            });
        }

        if (stats?.Cancelled > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Title = "Cancelaciones del día",
                Description = $"{stats.Cancelled} cita(s) canceladas hoy.",
                Severity = "info",
                Time = now,
                ActionLabel = "Ver agenda",
                ActionHref = "/agenda"
            });
        }

        if (data.WaitingCount > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Title = "Pacientes en espera",
                Description = $"{data.WaitingCount} paciente(s) confirmados cuya hora ya pasó.",
                Severity = data.WaitingCount >= 3 ? "warning" : "info",
                Time = now,
                ActionLabel = "Ver cola",
                ActionHref = "/agenda"
            });
        }

        if (data.UpcomingDues.Count > 0)
        {
            var total = data.UpcomingDues.Sum(p => p.Balance);
            alerts.Add(new DashboardAlert
            {
                Title = "Saldos pendientes",
                Description = $"{data.UpcomingDues.Count} factura(s) con saldo por cobrar · {total.ToString("C0", EsDo)}.",
                Severity = "warning",
                Time = now,
                ActionLabel = "Cobrar",
                ActionHref = "/facturacion"
            });
        }

        if (data.PendingTreatmentsCount > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Title = "Tratamientos abiertos",
                Description = $"{data.PendingTreatmentsCount} tratamiento(s) planificados o en curso.",
                Severity = "info",
                Time = now,
                ActionLabel = "Ver tratamientos",
                ActionHref = "/tratamientos-clinicos"
            });
        }

        if (data.DailyRevenue > 0)
        {
            alerts.Add(new DashboardAlert
            {
                Title = "Cobros registrados",
                Description = $"{data.TodayCollections.Count} cobro(s) hoy · {data.DailyRevenue.ToString("C0", EsDo)}.",
                Severity = "success",
                Time = now,
                ActionLabel = "Ver facturación",
                ActionHref = "/facturacion"
            });
        }

        if (alerts.Count == 0)
        {
            alerts.Add(new DashboardAlert
            {
                Title = "Sin alertas",
                Description = "No hay incidencias operativas para mostrar ahora.",
                Severity = "success",
                Time = now
            });
        }

        return alerts;
    }

    private static bool Needs(HashSet<string> keys, params string[] widgetKeys) =>
        widgetKeys.Any(keys.Contains);

    private static Task<(List<AppointmentAgendaItem> Items, string? Error)> CompletedAgenda() =>
        Task.FromResult((new List<AppointmentAgendaItem>(), (string?)null));

    private static List<int> BuildWeekCounts(List<AppointmentAgendaItem> agenda, DateTime weekStart)
    {
        var counts = new int[7];
        foreach (var item in agenda)
        {
            if (item.ParsedDate is not DateTime date)
                continue;
            var index = (int)((date.Date - weekStart.Date).TotalDays);
            if (index is >= 0 and < 7)
                counts[index]++;
        }
        return counts.ToList();
    }

    private static List<TreatmentMixSlice> BuildMix(List<DentalTreatmentListItem> treatments, bool restricted)
    {
        if (restricted || treatments.Count == 0)
            return [];

        var groups = treatments
            .GroupBy(t => string.IsNullOrWhiteSpace(t.ProcedureName) ? "Otros" : t.ProcedureName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(6)
            .ToList();
        var total = groups.Sum(g => g.Count);
        if (total == 0)
            return [];
        return groups.Select(g => new TreatmentMixSlice
        {
            Name = g.Name,
            Count = g.Count,
            Percent = (int)Math.Round(g.Count * 100.0 / total)
        }).ToList();
    }

    private static decimal SumCollected(List<PaymentListItem> items, DateTime from, DateTime to) =>
        items.Where(p =>
            {
                var date = ParseDate(p.PaymentDate) ?? ParseDate(p.AppointmentDate);
                return IsCollected(p) && date?.Date >= from && date?.Date <= to;
            })
            .Sum(CollectedAmount);

    private static bool IsTerminalStatus(string status) =>
        status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
        || status.Equals("NoShow", StringComparison.OrdinalIgnoreCase);

    private static bool IsCollected(PaymentListItem payment) =>
        payment.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase)
        || payment.PaymentStatus.Equals("PartiallyPaid", StringComparison.OrdinalIgnoreCase);

    private static decimal CollectedAmount(PaymentListItem payment) =>
        payment.AmountCollected > 0 ? payment.AmountCollected : payment.Amount;

    private static bool SameDay(PaymentListItem payment, DateTime day)
    {
        var date = ParseDate(payment.PaymentDate) ?? ParseDate(payment.AppointmentDate);
        return date?.Date == day.Date;
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
            return parsed;
        return null;
    }

    private static bool IsRestricted(string? error) =>
        !string.IsNullOrWhiteSpace(error)
        && (error.Contains("403", StringComparison.OrdinalIgnoreCase)
            || error.Contains("Forbidden", StringComparison.OrdinalIgnoreCase)
            || error.Contains("autorizad", StringComparison.OrdinalIgnoreCase)
            || error.Contains("not authorized", StringComparison.OrdinalIgnoreCase)
            || error.Contains("Access Denied", StringComparison.OrdinalIgnoreCase)
            || error.Contains("Only admins", StringComparison.OrdinalIgnoreCase));

    private static bool IsConnectionError(string? error) =>
        !string.IsNullOrWhiteSpace(error)
        && (error.Contains("No se pudo conectar", StringComparison.OrdinalIgnoreCase)
            || error.Contains("Error de conexión", StringComparison.OrdinalIgnoreCase));
}
