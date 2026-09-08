namespace Clinic_System.Core.Reports;

public sealed class ClinicReportSnapshot
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public AttendanceSnapshot Attendance { get; set; } = new();
    public RevenueSnapshot Revenue { get; set; } = new();
    public List<DoctorProductionRow> DoctorProduction { get; set; } = [];
    public PatientSnapshot Patients { get; set; } = new();
    public TreatmentSnapshot Treatments { get; set; } = new();
    public InventorySnapshot Inventory { get; set; } = new();
    public List<ReceivableRow> Receivables { get; set; } = [];
}

public sealed class AttendanceSnapshot
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Confirmed { get; set; }
    public int Pending { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public int Rescheduled { get; set; }
    public List<AttendanceByDoctorRow> ByDoctor { get; set; } = [];
}

public sealed class AttendanceByDoctorRow
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Confirmed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
}

public sealed class RevenueSnapshot
{
    public decimal Total { get; set; }
    public decimal Cash { get; set; }
    public decimal Card { get; set; }
    public decimal InstaPay { get; set; }
    public int TransactionCount { get; set; }
    public decimal OutstandingBalance { get; set; }
}

public sealed class DoctorProductionRow
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int CompletedAppointments { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class PatientSnapshot
{
    public int TotalPatients { get; set; }
    public int NewInPeriod { get; set; }
    public int ReturningInPeriod { get; set; }
}

public sealed class TreatmentSnapshot
{
    public int Planned { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int PendingPlans { get; set; }
    public List<ProcedureCountRow> TopProcedures { get; set; } = [];
}

public sealed class ProcedureCountRow
{
    public string ProcedureName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class InventorySnapshot
{
    public int LowStockCount { get; set; }
    public List<LowStockRow> LowStockItems { get; set; } = [];
    public int MovementsIn { get; set; }
    public int MovementsOut { get; set; }
    public decimal QuantityOut { get; set; }
    public List<ConsumptionByProcedureRow> ConsumptionByProcedure { get; set; } = [];
}

public sealed class LowStockRow
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal MinimumStock { get; set; }
}

public sealed class ConsumptionByProcedureRow
{
    public string ProcedureName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public sealed class ReceivableRow
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int InvoiceCount { get; set; }
}
