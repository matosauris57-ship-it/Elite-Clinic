namespace DentalCare.Admin.Models;

public class ClinicReportsData
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public AttendanceReportData Attendance { get; set; } = new();
    public RevenueReportData Revenue { get; set; } = new();
    public List<DoctorProductionReportData> DoctorProduction { get; set; } = [];
    public PatientReportData Patients { get; set; } = new();
    public TreatmentReportData Treatments { get; set; } = new();
    public InventoryReportData Inventory { get; set; } = new();
    public List<ReceivableReportData> Receivables { get; set; } = [];
}

public class AttendanceReportData
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Confirmed { get; set; }
    public int Pending { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public int Rescheduled { get; set; }
    public decimal AttendanceRate { get; set; }
    public decimal NoShowRate { get; set; }
    public List<AttendanceByDoctorData> ByDoctor { get; set; } = [];
}

public class AttendanceByDoctorData
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Confirmed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public decimal AttendanceRate { get; set; }
}

public class RevenueReportData
{
    public decimal Total { get; set; }
    public string TotalDisplay { get; set; } = string.Empty;
    public decimal Cash { get; set; }
    public string CashDisplay { get; set; } = string.Empty;
    public decimal Card { get; set; }
    public string CardDisplay { get; set; } = string.Empty;
    public decimal InstaPay { get; set; }
    public string InstaPayDisplay { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string OutstandingBalanceDisplay { get; set; } = string.Empty;
}

public class DoctorProductionReportData
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int CompletedAppointments { get; set; }
    public decimal Revenue { get; set; }
    public string RevenueDisplay { get; set; } = string.Empty;
}

public class PatientReportData
{
    public int TotalPatients { get; set; }
    public int NewInPeriod { get; set; }
    public int ReturningInPeriod { get; set; }
}

public class TreatmentReportData
{
    public int Planned { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int PendingPlans { get; set; }
    public List<ProcedureCountData> TopProcedures { get; set; } = [];
}

public class ProcedureCountData
{
    public string ProcedureName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class InventoryReportData
{
    public int LowStockCount { get; set; }
    public int MovementsIn { get; set; }
    public int MovementsOut { get; set; }
    public decimal QuantityOut { get; set; }
    public List<LowStockItemReportData> LowStockItems { get; set; } = [];
    public List<ConsumptionByProcedureData> ConsumptionByProcedure { get; set; } = [];
}

public class LowStockItemReportData
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal MinimumStock { get; set; }
}

public class ConsumptionByProcedureData
{
    public string ProcedureName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class ReceivableReportData
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string BalanceDisplay { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
}
