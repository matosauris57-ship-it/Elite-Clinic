using Clinic_System.Core.Finance;
using Clinic_System.Core.Reports;

namespace Clinic_System.Application.DTOs.Reports;

public sealed class ClinicReportsDTO
{
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public AttendanceReportDTO Attendance { get; set; } = new();
    public RevenueReportDTO Revenue { get; set; } = new();
    public List<DoctorProductionReportDTO> DoctorProduction { get; set; } = [];
    public PatientReportDTO Patients { get; set; } = new();
    public TreatmentReportDTO Treatments { get; set; } = new();
    public InventoryReportDTO Inventory { get; set; } = new();
    public List<ReceivableReportDTO> Receivables { get; set; } = [];

    public static ClinicReportsDTO FromSnapshot(ClinicReportSnapshot snapshot)
    {
        static string MoneyDisplay(decimal value) => Money.Format(value);

        static decimal Rate(int part, int total) =>
            total <= 0 ? 0 : Math.Round(100m * part / total, 1, MidpointRounding.AwayFromZero);

        return new ClinicReportsDTO
        {
            FromDate = snapshot.FromDate.ToString("yyyy-MM-dd"),
            ToDate = snapshot.ToDate.ToString("yyyy-MM-dd"),
            Attendance = new AttendanceReportDTO
            {
                Total = snapshot.Attendance.Total,
                Completed = snapshot.Attendance.Completed,
                Confirmed = snapshot.Attendance.Confirmed,
                Pending = snapshot.Attendance.Pending,
                Cancelled = snapshot.Attendance.Cancelled,
                NoShow = snapshot.Attendance.NoShow,
                Rescheduled = snapshot.Attendance.Rescheduled,
                AttendanceRate = Rate(
                    snapshot.Attendance.Completed + snapshot.Attendance.Confirmed,
                    snapshot.Attendance.Total),
                NoShowRate = Rate(snapshot.Attendance.NoShow, snapshot.Attendance.Total),
                ByDoctor = snapshot.Attendance.ByDoctor.Select(d => new AttendanceByDoctorDTO
                {
                    DoctorId = d.DoctorId,
                    DoctorName = d.DoctorName,
                    Total = d.Total,
                    Completed = d.Completed,
                    Confirmed = d.Confirmed,
                    Cancelled = d.Cancelled,
                    NoShow = d.NoShow,
                    AttendanceRate = Rate(d.Completed + d.Confirmed, d.Total)
                }).ToList()
            },
            Revenue = new RevenueReportDTO
            {
                Total = snapshot.Revenue.Total,
                TotalDisplay = MoneyDisplay(snapshot.Revenue.Total),
                Cash = snapshot.Revenue.Cash,
                CashDisplay = MoneyDisplay(snapshot.Revenue.Cash),
                Card = snapshot.Revenue.Card,
                CardDisplay = MoneyDisplay(snapshot.Revenue.Card),
                InstaPay = snapshot.Revenue.InstaPay,
                InstaPayDisplay = MoneyDisplay(snapshot.Revenue.InstaPay),
                TransactionCount = snapshot.Revenue.TransactionCount,
                OutstandingBalance = snapshot.Revenue.OutstandingBalance,
                OutstandingBalanceDisplay = MoneyDisplay(snapshot.Revenue.OutstandingBalance)
            },
            DoctorProduction = snapshot.DoctorProduction.Select(d => new DoctorProductionReportDTO
            {
                DoctorId = d.DoctorId,
                DoctorName = d.DoctorName,
                CompletedAppointments = d.CompletedAppointments,
                Revenue = d.Revenue,
                RevenueDisplay = MoneyDisplay(d.Revenue)
            }).ToList(),
            Patients = new PatientReportDTO
            {
                TotalPatients = snapshot.Patients.TotalPatients,
                NewInPeriod = snapshot.Patients.NewInPeriod,
                ReturningInPeriod = snapshot.Patients.ReturningInPeriod
            },
            Treatments = new TreatmentReportDTO
            {
                Planned = snapshot.Treatments.Planned,
                InProgress = snapshot.Treatments.InProgress,
                Completed = snapshot.Treatments.Completed,
                Cancelled = snapshot.Treatments.Cancelled,
                PendingPlans = snapshot.Treatments.PendingPlans,
                TopProcedures = snapshot.Treatments.TopProcedures.Select(p => new ProcedureCountDTO
                {
                    ProcedureName = p.ProcedureName,
                    Count = p.Count
                }).ToList()
            },
            Inventory = new InventoryReportDTO
            {
                LowStockCount = snapshot.Inventory.LowStockCount,
                MovementsIn = snapshot.Inventory.MovementsIn,
                MovementsOut = snapshot.Inventory.MovementsOut,
                QuantityOut = snapshot.Inventory.QuantityOut,
                LowStockItems = snapshot.Inventory.LowStockItems.Select(i => new LowStockItemReportDTO
                {
                    Id = i.Id,
                    Sku = i.Sku,
                    Name = i.Name,
                    Unit = i.Unit,
                    QuantityOnHand = i.QuantityOnHand,
                    MinimumStock = i.MinimumStock
                }).ToList(),
                ConsumptionByProcedure = snapshot.Inventory.ConsumptionByProcedure.Select(c => new ConsumptionByProcedureDTO
                {
                    ProcedureName = c.ProcedureName,
                    ItemName = c.ItemName,
                    Unit = c.Unit,
                    Quantity = c.Quantity
                }).ToList()
            },
            Receivables = snapshot.Receivables.Select(r => new ReceivableReportDTO
            {
                PatientId = r.PatientId,
                PatientName = r.PatientName,
                Balance = r.Balance,
                BalanceDisplay = MoneyDisplay(r.Balance),
                InvoiceCount = r.InvoiceCount
            }).ToList()
        };
    }
}

public sealed class AttendanceReportDTO
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
    public List<AttendanceByDoctorDTO> ByDoctor { get; set; } = [];
}

public sealed class AttendanceByDoctorDTO
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

public sealed class RevenueReportDTO
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

public sealed class DoctorProductionReportDTO
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int CompletedAppointments { get; set; }
    public decimal Revenue { get; set; }
    public string RevenueDisplay { get; set; } = string.Empty;
}

public sealed class PatientReportDTO
{
    public int TotalPatients { get; set; }
    public int NewInPeriod { get; set; }
    public int ReturningInPeriod { get; set; }
}

public sealed class TreatmentReportDTO
{
    public int Planned { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int PendingPlans { get; set; }
    public List<ProcedureCountDTO> TopProcedures { get; set; } = [];
}

public sealed class ProcedureCountDTO
{
    public string ProcedureName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class InventoryReportDTO
{
    public int LowStockCount { get; set; }
    public int MovementsIn { get; set; }
    public int MovementsOut { get; set; }
    public decimal QuantityOut { get; set; }
    public List<LowStockItemReportDTO> LowStockItems { get; set; } = [];
    public List<ConsumptionByProcedureDTO> ConsumptionByProcedure { get; set; } = [];
}

public sealed class LowStockItemReportDTO
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal MinimumStock { get; set; }
}

public sealed class ConsumptionByProcedureDTO
{
    public string ProcedureName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public sealed class ReceivableReportDTO
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string BalanceDisplay { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
}
