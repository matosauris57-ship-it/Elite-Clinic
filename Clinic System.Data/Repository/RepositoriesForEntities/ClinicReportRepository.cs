using Clinic_System.Core.Enums;
using Clinic_System.Core.Reports;
using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class ClinicReportRepository : IClinicReportRepository
{
    private readonly AppDbContext _db;

    public ClinicReportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClinicReportSnapshot> GetSnapshotAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate.Date;
        var toExclusive = toDate.Date.AddDays(1);
        var toInclusive = toExclusive.AddTicks(-1);

        var snapshot = new ClinicReportSnapshot
        {
            FromDate = from,
            ToDate = toDate.Date
        };

        await FillAttendanceAsync(snapshot, from, toExclusive, cancellationToken);
        await FillRevenueAsync(snapshot, from, toInclusive, cancellationToken);
        await FillDoctorProductionAsync(snapshot, from, toExclusive, toInclusive, cancellationToken);
        await FillPatientsAsync(snapshot, from, toExclusive, cancellationToken);
        await FillTreatmentsAsync(snapshot, from, toExclusive, cancellationToken);
        await FillInventoryAsync(snapshot, from, toExclusive, cancellationToken);
        await FillReceivablesAsync(snapshot, cancellationToken);

        return snapshot;
    }

    private async Task FillAttendanceAsync(
        ClinicReportSnapshot snapshot,
        DateTime from,
        DateTime toExclusive,
        CancellationToken cancellationToken)
    {
        var appointments = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentDate >= from && a.AppointmentDate < toExclusive)
            .Select(a => new { a.Status, a.DoctorId, DoctorName = a.Doctor.FullName })
            .ToListAsync(cancellationToken);

        var attendance = snapshot.Attendance;
        attendance.Total = appointments.Count;
        attendance.Completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        attendance.Confirmed = appointments.Count(a => a.Status == AppointmentStatus.Confirmed);
        attendance.Pending = appointments.Count(a => a.Status == AppointmentStatus.Pending);
        attendance.Cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        attendance.NoShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow);
        attendance.Rescheduled = appointments.Count(a => a.Status == AppointmentStatus.Rescheduled);

        attendance.ByDoctor = appointments
            .GroupBy(a => new { a.DoctorId, a.DoctorName })
            .Select(g => new AttendanceByDoctorRow
            {
                DoctorId = g.Key.DoctorId,
                DoctorName = g.Key.DoctorName,
                Total = g.Count(),
                Completed = g.Count(x => x.Status == AppointmentStatus.Completed),
                Confirmed = g.Count(x => x.Status == AppointmentStatus.Confirmed),
                Cancelled = g.Count(x => x.Status == AppointmentStatus.Cancelled),
                NoShow = g.Count(x => x.Status == AppointmentStatus.NoShow)
            })
            .OrderByDescending(x => x.Total)
            .ToList();
    }

    private async Task FillRevenueAsync(
        ClinicReportSnapshot snapshot,
        DateTime from,
        DateTime toInclusive,
        CancellationToken cancellationToken)
    {
        var receiptStats = await _db.PaymentReceipts
            .AsNoTracking()
            .Where(r => r.PaidAt >= from && r.PaidAt <= toInclusive)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Sum(r => r.Amount),
                Cash = g.Where(r => r.PaymentMethod == PaymentMethod.Cash).Sum(r => r.Amount),
                Card = g.Where(r => r.PaymentMethod == PaymentMethod.CreditCard).Sum(r => r.Amount),
                Insta = g.Where(r => r.PaymentMethod == PaymentMethod.InstaPay).Sum(r => r.Amount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        snapshot.Revenue.Total = receiptStats?.Total ?? 0;
        snapshot.Revenue.Cash = receiptStats?.Cash ?? 0;
        snapshot.Revenue.Card = receiptStats?.Card ?? 0;
        snapshot.Revenue.InstaPay = receiptStats?.Insta ?? 0;
        snapshot.Revenue.TransactionCount = receiptStats?.Count ?? 0;
    }

    private async Task FillDoctorProductionAsync(
        ClinicReportSnapshot snapshot,
        DateTime from,
        DateTime toExclusive,
        DateTime toInclusive,
        CancellationToken cancellationToken)
    {
        var completed = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentDate >= from
                && a.AppointmentDate < toExclusive
                && a.Status == AppointmentStatus.Completed)
            .GroupBy(a => new { a.DoctorId, DoctorName = a.Doctor.FullName })
            .Select(g => new
            {
                g.Key.DoctorId,
                g.Key.DoctorName,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var revenues = await _db.PaymentReceipts
            .AsNoTracking()
            .Where(r => r.PaidAt >= from && r.PaidAt <= toInclusive)
            .GroupBy(r => new { r.Payment.Appointment.DoctorId, DoctorName = r.Payment.Appointment.Doctor.FullName })
            .Select(g => new
            {
                g.Key.DoctorId,
                g.Key.DoctorName,
                Revenue = g.Sum(r => r.Amount)
            })
            .ToListAsync(cancellationToken);

        var doctorIds = completed.Select(c => c.DoctorId)
            .Union(revenues.Select(r => r.DoctorId))
            .Distinct();

        snapshot.DoctorProduction = doctorIds
            .Select(id =>
            {
                var c = completed.FirstOrDefault(x => x.DoctorId == id);
                var r = revenues.FirstOrDefault(x => x.DoctorId == id);
                return new DoctorProductionRow
                {
                    DoctorId = id,
                    DoctorName = c?.DoctorName ?? r?.DoctorName ?? $"Médico #{id}",
                    CompletedAppointments = c?.Count ?? 0,
                    Revenue = r?.Revenue ?? 0
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.CompletedAppointments)
            .ToList();
    }

    private async Task FillPatientsAsync(
        ClinicReportSnapshot snapshot,
        DateTime from,
        DateTime toExclusive,
        CancellationToken cancellationToken)
    {
        snapshot.Patients.TotalPatients = await _db.Patients.AsNoTracking().CountAsync(cancellationToken);
        snapshot.Patients.NewInPeriod = await _db.Patients
            .AsNoTracking()
            .CountAsync(p => p.CreatedAt >= from && p.CreatedAt < toExclusive, cancellationToken);

        snapshot.Patients.ReturningInPeriod = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentDate >= from && a.AppointmentDate < toExclusive)
            .Where(a => a.Patient.CreatedAt < from)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    private async Task FillTreatmentsAsync(
        ClinicReportSnapshot snapshot,
        DateTime from,
        DateTime toExclusive,
        CancellationToken cancellationToken)
    {
        var treatments = _db.DentalTreatments
            .AsNoTracking()
            .Where(t => (t.PerformedDate ?? t.CreatedAt) >= from
                && (t.PerformedDate ?? t.CreatedAt) < toExclusive);

        var statusCounts = await treatments
            .GroupBy(t => t.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        snapshot.Treatments.Planned = statusCounts.FirstOrDefault(x => x.Key == DentalTreatmentStatus.Planned)?.Count ?? 0;
        snapshot.Treatments.InProgress = statusCounts.FirstOrDefault(x => x.Key == DentalTreatmentStatus.InProgress)?.Count ?? 0;
        snapshot.Treatments.Completed = statusCounts.FirstOrDefault(x => x.Key == DentalTreatmentStatus.Completed)?.Count ?? 0;
        snapshot.Treatments.Cancelled = statusCounts.FirstOrDefault(x => x.Key == DentalTreatmentStatus.Cancelled)?.Count ?? 0;

        snapshot.Treatments.PendingPlans = await _db.TreatmentPlans
            .AsNoTracking()
            .CountAsync(p => p.Status == TreatmentPlanStatus.Draft || p.Status == TreatmentPlanStatus.Approved, cancellationToken);

        snapshot.Treatments.TopProcedures = await treatments
            .GroupBy(t => t.ProcedureName)
            .Select(g => new ProcedureCountRow
            {
                ProcedureName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    private async Task FillInventoryAsync(
        ClinicReportSnapshot snapshot,
        DateTime from,
        DateTime toExclusive,
        CancellationToken cancellationToken)
    {
        var lowStock = await _db.InventoryItems
            .AsNoTracking()
            .Where(i => i.IsActive && i.QuantityOnHand <= i.MinimumStock)
            .OrderBy(i => i.Name)
            .Select(i => new LowStockRow
            {
                Id = i.Id,
                Sku = i.Sku,
                Name = i.Name,
                Unit = i.Unit,
                QuantityOnHand = i.QuantityOnHand,
                MinimumStock = i.MinimumStock
            })
            .ToListAsync(cancellationToken);

        snapshot.Inventory.LowStockItems = lowStock;
        snapshot.Inventory.LowStockCount = lowStock.Count;

        var movements = await _db.StockMovements
            .AsNoTracking()
            .Where(m => m.CreatedAt >= from && m.CreatedAt < toExclusive)
            .GroupBy(m => m.Type)
            .Select(g => new { g.Key, Count = g.Count(), Qty = g.Sum(m => m.Quantity) })
            .ToListAsync(cancellationToken);

        snapshot.Inventory.MovementsIn = movements.FirstOrDefault(x => x.Key == StockMovementType.In)?.Count ?? 0;
        snapshot.Inventory.MovementsOut = movements.FirstOrDefault(x => x.Key == StockMovementType.Out)?.Count ?? 0;
        snapshot.Inventory.QuantityOut = movements.FirstOrDefault(x => x.Key == StockMovementType.Out)?.Qty ?? 0;

        snapshot.Inventory.ConsumptionByProcedure = await _db.TreatmentMaterialConsumptionLines
            .AsNoTracking()
            .Where(l => l.IsIncluded
                && l.Consumption.ConfirmedAt >= from
                && l.Consumption.ConfirmedAt < toExclusive)
            .GroupBy(l => new
            {
                ProcedureName = l.Consumption.DentalTreatment.ProcedureName,
                ItemName = l.InventoryItem.Name,
                l.InventoryItem.Unit
            })
            .Select(g => new ConsumptionByProcedureRow
            {
                ProcedureName = g.Key.ProcedureName,
                ItemName = g.Key.ItemName,
                Unit = g.Key.Unit,
                Quantity = g.Sum(x => x.ActualQuantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    private async Task FillReceivablesAsync(ClinicReportSnapshot snapshot, CancellationToken cancellationToken)
    {
        var openStatuses = new[]
        {
            PaymentStatus.Pending,
            PaymentStatus.PartiallyPaid,
            PaymentStatus.Failed
        };

        var rows = await _db.Payments
            .AsNoTracking()
            .Where(p => openStatuses.Contains(p.PaymentStatus))
            .Select(p => new
            {
                p.Appointment.PatientId,
                PatientName = p.Appointment.Patient.FullName,
                InvoiceTotal = p.InvoiceLines.Any()
                    ? p.InvoiceLines.Sum(l => l.UnitPrice * l.Quantity)
                    : p.AmountPaid,
                Collected = p.Receipts.Sum(r => r.Amount)
            })
            .ToListAsync(cancellationToken);

        var receivables = rows
            .Select(r => new
            {
                r.PatientId,
                r.PatientName,
                Balance = Math.Max(0, decimal.Round(r.InvoiceTotal - r.Collected, 2, MidpointRounding.AwayFromZero))
            })
            .Where(r => r.Balance > 0)
            .GroupBy(r => new { r.PatientId, r.PatientName })
            .Select(g => new ReceivableRow
            {
                PatientId = g.Key.PatientId,
                PatientName = g.Key.PatientName,
                Balance = g.Sum(x => x.Balance),
                InvoiceCount = g.Count()
            })
            .OrderByDescending(x => x.Balance)
            .Take(50)
            .ToList();

        snapshot.Receivables = receivables;
        snapshot.Revenue.OutstandingBalance = receivables.Sum(r => r.Balance);
    }
}
