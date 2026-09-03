namespace Clinic_System.Core.Dashboard;

public static class DashboardWidgetCatalog
{
    public const int Columns = 12;

    public static readonly IReadOnlyList<DashboardWidgetDefinition> All =
    [
        Def(DashboardWidgetKeys.AppointmentsTodayKpi, "Citas de hoy", "Total de citas activas del día.", "agenda.view", false, 0, 0, 3, 2, 2, 2, 6, 4, 2, 2, 3, 2, 6, 4),
        Def(DashboardWidgetKeys.WaitingCountKpi, "Pacientes esperando", "Confirmados cuya hora ya pasó.", "agenda.view", false, 3, 0, 3, 2, 2, 2, 6, 4, 2, 2, 3, 2, 6, 4),
        Def(DashboardWidgetKeys.DailyRevenueKpi, "Cobros del día", "Total cobrado hoy.", "facturacion.view", false, 6, 0, 3, 2, 2, 2, 6, 4, 2, 2, 3, 2, 6, 4),
        Def(DashboardWidgetKeys.PendingTreatmentsKpi, "Tratamientos pendientes", "Planificados o en curso.", "tratamientos.view", false, 9, 0, 3, 2, 2, 2, 6, 4, 2, 2, 3, 2, 6, 4),
        Def(DashboardWidgetKeys.TodayAppointments, "Citas de hoy (lista)", "Agenda activa ordenada por hora.", "agenda.view", false, 0, 2, 5, 4, 3, 3, 12, 8, 4, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.WaitingQueue, "Cola de espera", "Pacientes confirmados en espera.", "agenda.view", false, 5, 2, 3, 4, 3, 3, 12, 8, 3, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.DailyCollections, "Cobros del día (detalle)", "Transacciones cobradas hoy.", "facturacion.view", false, 8, 2, 4, 4, 3, 3, 12, 8, 3, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.MonthlyIncomeChart, "Ingresos del año", "Cobros agrupados por mes.", "facturacion.view", false, 0, 6, 8, 4, 4, 3, 12, 8, 6, 3, 8, 4, 12, 6),
        Def(DashboardWidgetKeys.SmartAlerts, "Alertas importantes", "Incidencias operativas del día.", "dashboard.view", false, 8, 6, 4, 4, 3, 3, 12, 8, 4, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.PendingTreatments, "Tratamientos abiertos", "Listado de tratamientos en curso o planificados.", "tratamientos.view", false, 0, 10, 6, 4, 3, 3, 12, 8, 4, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.UpcomingDues, "Saldos pendientes", "Facturas con saldo por cobrar.", "facturacion.view", false, 6, 10, 6, 4, 3, 3, 12, 8, 4, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.PatientsRegistered, "Pacientes registrados", "Total de pacientes en la clínica.", "pacientes.view", false, 0, 14, 3, 2, 2, 2, 6, 4),
        Def(DashboardWidgetKeys.PatientsNew, "Pacientes nuevos", "Altas del mes frente al mes anterior.", "pacientes.view", false, 3, 14, 3, 2, 2, 2, 6, 4),
        Def(DashboardWidgetKeys.UpcomingAppointments, "Próximas citas", "Citas siguientes con paciente y odontólogo.", "agenda.view", false, 6, 14, 6, 4, 4, 3, 12, 8, 4, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.AppointmentsCompleted, "Citas completadas", "Citas completadas en el periodo.", "agenda.view", false, 0, 18, 3, 2, 2, 2, 6, 4),
        Def(DashboardWidgetKeys.AppointmentsCancelled, "Citas canceladas", "Cancelaciones del día.", "agenda.view", false, 3, 18, 3, 2, 2, 2, 6, 4),
        Def(DashboardWidgetKeys.AppointmentsNoShow, "Citas no asistidas", "No-shows del día.", "agenda.view", false, 6, 18, 3, 2, 2, 2, 6, 4),
        Def(DashboardWidgetKeys.AppointmentsWeekChart, "Citas de la semana", "Distribución diaria de la semana actual.", "agenda.view", false, 0, 20, 12, 4, 4, 3, 12, 8, 6, 3, 8, 4, 12, 5),
        Def(DashboardWidgetKeys.TreatmentsCompleted, "Tratamientos completados", "Tratamientos finalizados.", "tratamientos.view", false, 9, 18, 3, 2, 2, 2, 6, 4),
        Def(DashboardWidgetKeys.TreatmentMix, "Distribución de tratamientos", "Mix real por procedimiento.", "tratamientos.view", false, 0, 24, 6, 4, 4, 3, 12, 8, 4, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.RevenueSummary, "Ingresos", "Total cobrado según el periodo elegido.", "facturacion.view", false, 6, 24, 3, 3, 2, 2, 8, 5),
        Def(DashboardWidgetKeys.PaymentMethods, "Métodos de pago", "Distribución de cobros del día.", "facturacion.view", false, 9, 24, 3, 3, 2, 2, 8, 5),
        Def(DashboardWidgetKeys.RecentActivity, "Actividad reciente", "Eventos clínicos recientes.", "historial.view", false, 0, 28, 6, 4, 4, 3, 12, 8, 4, 3, 6, 4, 12, 6),
        Def(DashboardWidgetKeys.PeriodontalIncomplete, "Periodontogramas incompletos", "Evaluaciones sin sitios registrados.", "periodontograma.view", false, 6, 28, 3, 2, 2, 2, 6, 4, 2, 2, 3, 2, 6, 4),
        Def(DashboardWidgetKeys.SystemStatus, "Estado del sistema", "Disponibilidad de la API clínica.", "dashboard.view", false, 9, 28, 3, 2, 2, 2, 6, 4, 2, 2, 3, 2, 6, 4)
    ];

    public static IReadOnlyDictionary<string, DashboardWidgetDefinition> ByKey { get; } =
        All.ToDictionary(d => d.Key, StringComparer.OrdinalIgnoreCase);

    public static bool TryGet(string key, out DashboardWidgetDefinition definition) =>
        ByKey.TryGetValue(key, out definition!);

    public static DashboardLayoutDocument CreateDefaultLayout()
    {
        var visible = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            DashboardWidgetKeys.AppointmentsTodayKpi,
            DashboardWidgetKeys.WaitingCountKpi,
            DashboardWidgetKeys.DailyRevenueKpi,
            DashboardWidgetKeys.PendingTreatmentsKpi,
            DashboardWidgetKeys.TodayAppointments,
            DashboardWidgetKeys.WaitingQueue,
            DashboardWidgetKeys.DailyCollections,
            DashboardWidgetKeys.MonthlyIncomeChart,
            DashboardWidgetKeys.SmartAlerts,
            DashboardWidgetKeys.PendingTreatments,
            DashboardWidgetKeys.UpcomingDues,
            DashboardWidgetKeys.SystemStatus
        };

        return new DashboardLayoutDocument
        {
            Version = 1,
            Items = All.Select(d => new DashboardWidgetPlacement
            {
                Id = d.Key,
                WidgetKey = d.Key,
                Visible = visible.Contains(d.Key) || d.Required,
                X = d.DefaultX,
                Y = d.DefaultY,
                W = d.DefaultW,
                H = d.DefaultH
            }).ToList()
        };
    }

    public static IReadOnlyList<string> FinancialWidgetKeys { get; } =
    [
        DashboardWidgetKeys.DailyRevenueKpi,
        DashboardWidgetKeys.DailyCollections,
        DashboardWidgetKeys.MonthlyIncomeChart,
        DashboardWidgetKeys.UpcomingDues,
        DashboardWidgetKeys.RevenueSummary,
        DashboardWidgetKeys.PaymentMethods
    ];

    private static DashboardWidgetDefinition Def(
        string key, string title, string description, string permission, bool required,
        int x, int y, int w, int h, int minW, int minH, int maxW, int maxH,
        int? smallW = null, int? smallH = null, int? medW = null, int? medH = null, int? largeW = null, int? largeH = null) =>
        new()
        {
            Key = key,
            Title = title,
            Description = description,
            Permission = permission,
            Required = required,
            DefaultX = x,
            DefaultY = y,
            DefaultW = w,
            DefaultH = h,
            MinW = minW,
            MinH = minH,
            MaxW = maxW,
            MaxH = maxH,
            Small = new DashboardWidgetSize { W = smallW ?? 3, H = smallH ?? 2 },
            Medium = new DashboardWidgetSize { W = medW ?? 6, H = medH ?? 3 },
            Large = new DashboardWidgetSize { W = largeW ?? 12, H = largeH ?? 5 }
        };
}
