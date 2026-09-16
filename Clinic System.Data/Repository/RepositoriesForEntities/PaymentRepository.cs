namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<Payment> Items, int TotalCount)> GetFilteredPaymentsAsync(
        int? doctorId,
        int? patientId,
        DateTime? fromDate,
        DateTime? toDate,
        PaymentStatus? status,
        PaymentMethod? method,
        int pageNumber,
        int pageSize,
        string? search = null,
        bool outstandingOnly = false)
        {
            var query = context.Payments
                .AsNoTracking()
                .Include(p => p.Receipts)
                .Include(p => p.InvoiceLines)
                .Include(p => p.Patient)
                .Include(p => p.TreatmentPlan)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                .AsQueryable();

            if (doctorId.HasValue)
                query = query.Where(p => p.Appointment != null && p.Appointment.DoctorId == doctorId);

            if (patientId.HasValue)
                query = query.Where(p => p.PatientId == patientId);

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(p =>
                    (p.PaymentDate ?? p.CreatedAt) >= start
                    || (p.Appointment != null && p.Appointment.AppointmentDate >= start));
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                query = query.Where(p =>
                    (p.PaymentDate ?? p.CreatedAt) < end
                    || (p.Appointment != null && p.Appointment.AppointmentDate < end));
            }

            if (outstandingOnly)
            {
                PaymentStatus[] outstanding =
                [
                    PaymentStatus.Pending,
                    PaymentStatus.Failed,
                    PaymentStatus.PartiallyPaid
                ];
                query = query.Where(p => outstanding.Contains(p.PaymentStatus));
            }
            else if (status.HasValue)
            {
                query = query.Where(p => p.PaymentStatus == status);
            }

            if (method.HasValue)
                query = query.Where(p => p.PaymentMethod == method);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                if (int.TryParse(term, out var paymentId))
                {
                    query = query.Where(p =>
                        p.Id == paymentId
                        || p.Patient.FullName.Contains(term)
                        || (p.Appointment != null && p.Appointment.Doctor.FullName.Contains(term)));
                }
                else
                {
                    query = query.Where(p =>
                        p.Patient.FullName.Contains(term)
                        || (p.Appointment != null && p.Appointment.Doctor.FullName.Contains(term)));
                }
            }

            query = query.OrderByDescending(p => p.PaymentDate ?? p.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Payment> GetPaymentDetailsByIdAsync(int id)
        {
            return await context.Payments
                .AsNoTracking()
                .Include(p => p.Receipts)
                .Include(p => p.InvoiceLines)
                .Include(p => p.Patient)
                .Include(p => p.TreatmentPlan)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment?> GetPaymentWithLinesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.Payments
                .Include(p => p.InvoiceLines)
                .Include(p => p.Receipts)
                .Include(p => p.Patient)
                .Include(p => p.TreatmentPlan)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Payment?> GetPaymentByAppointmentIdAsync(int appointmentId)
        {
            return await context.Payments
                .Include(p => p.InvoiceLines)
                .Include(p => p.Receipts)
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        }

        public async Task<(decimal total, decimal cash, decimal insta, decimal card, int count)> GetDailyTotalsAsync(DateTime date , CancellationToken cancellationToken = default)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            var receipts = context.PaymentReceipts
                 .AsNoTracking()
                 .Where(r => r.PaidAt >= start && r.PaidAt < end && !r.IsVoided);

            var stats = await receipts
                 .GroupBy(r => 1)
                 .Select(g => new
                 {
                     Total = g.Sum(r => r.Kind == PaymentReceiptKind.Refund ? -r.Amount : r.Amount),
                     Cash = g.Where(r => r.PaymentMethod == PaymentMethod.Cash)
                         .Sum(r => r.Kind == PaymentReceiptKind.Refund ? -r.Amount : r.Amount),
                     Card = g.Where(r => r.PaymentMethod == PaymentMethod.CreditCard)
                         .Sum(r => r.Kind == PaymentReceiptKind.Refund ? -r.Amount : r.Amount),
                     Insta = g.Where(r => r.PaymentMethod == PaymentMethod.InstaPay)
                         .Sum(r => r.Kind == PaymentReceiptKind.Refund ? -r.Amount : r.Amount),
                     Count = g.Count(r => r.Kind == PaymentReceiptKind.Payment)
                 })
                 .FirstOrDefaultAsync(cancellationToken);

            if (stats == null)
                return (0, 0, 0, 0, 0);

            return (stats.Total, stats.Cash, stats.Insta, stats.Card, stats.Count);
        }

        public async Task<(decimal total, int count)> GetDoctorRevenueStatsAsync(int doctorId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            var stats = await context.PaymentReceipts
                .AsNoTracking()
                .Where(r => !r.IsVoided
                            && r.Payment.Appointment.DoctorId == doctorId
                            && r.PaidAt >= from
                            && r.PaidAt <= to)
                .GroupBy(r => 1)
                .Select(g => new
                {
                    Total = g.Sum(r => r.Kind == PaymentReceiptKind.Refund ? -r.Amount : r.Amount),
                    Count = g.Count(r => r.Kind == PaymentReceiptKind.Payment)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stats == null)
                return (0, 0);

            return (stats.Total, stats.Count);
        }

        public async Task<Dictionary<int, decimal>> GetOutstandingBalancesByPatientAsync(CancellationToken cancellationToken = default)
            => await GetOutstandingBalancesByPatientAsync(patientIds: null, cancellationToken);

        public async Task<Dictionary<int, decimal>> GetOutstandingBalancesByPatientAsync(
            IEnumerable<int>? patientIds,
            CancellationToken cancellationToken = default)
        {
            PaymentStatus[] outstanding =
            [
                PaymentStatus.Pending,
                PaymentStatus.Failed,
                PaymentStatus.PartiallyPaid
            ];

            var idList = patientIds?.Distinct().ToList();

            var query = context.Payments
                .AsNoTracking()
                .Where(p => outstanding.Contains(p.PaymentStatus));

            if (idList is { Count: > 0 })
                query = query.Where(p => idList.Contains(p.PatientId));

            var payments = await query
                .Include(p => p.Receipts)
                .Include(p => p.InvoiceLines)
                .ToListAsync(cancellationToken);

            return payments
                .Where(p => p.Balance > 0)
                .GroupBy(p => p.PatientId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Balance));
        }
    }
}
