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
        string? search = null)
        {
            var query = context.Payments
                .AsNoTracking()
                .Include(p => p.Receipts)
                .Include(p => p.InvoiceLines)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                .AsQueryable();

            if (doctorId.HasValue)
                query = query.Where(p => p.Appointment.DoctorId == doctorId);

            if (patientId.HasValue)
                query = query.Where(p => p.Appointment.PatientId == patientId);

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(p =>
                    (p.PaymentDate ?? p.CreatedAt) >= start
                    || p.Appointment.AppointmentDate >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                query = query.Where(p =>
                    (p.PaymentDate ?? p.CreatedAt) < end
                    || p.Appointment.AppointmentDate < end);
            }

            if (status.HasValue)
                query = query.Where(p => p.PaymentStatus == status);

            if (method.HasValue)
                query = query.Where(p => p.PaymentMethod == method);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                if (int.TryParse(term, out var paymentId))
                {
                    query = query.Where(p =>
                        p.Id == paymentId
                        || p.Appointment.Patient.FullName.Contains(term)
                        || p.Appointment.Doctor.FullName.Contains(term));
                }
                else
                {
                    query = query.Where(p =>
                        p.Appointment.Patient.FullName.Contains(term)
                        || p.Appointment.Doctor.FullName.Contains(term));
                }
            }

            query = query.OrderByDescending(p => p.PaymentDate ?? p.CreatedAt);

            // 4. Get Total Count (قبل الـ Pagination)
            var totalCount = await query.CountAsync();

            // 5. Pagination
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
                 .Where(r => r.PaidAt >= start && r.PaidAt < end);

            var stats = await receipts
                 .GroupBy(r => 1)
                 .Select(g => new
                 {
                     Total = g.Sum(r => r.Amount),
                     Cash = g.Where(r => r.PaymentMethod == PaymentMethod.Cash).Sum(r => r.Amount),
                     Card = g.Where(r => r.PaymentMethod == PaymentMethod.CreditCard).Sum(r => r.Amount),
                     Insta = g.Where(r => r.PaymentMethod == PaymentMethod.InstaPay).Sum(r => r.Amount),
                     Count = g.Count()
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
                .Where(r => r.Payment.Appointment.DoctorId == doctorId &&
                            r.PaidAt >= from &&
                            r.PaidAt <= to)
                .GroupBy(r => 1)
                .Select(g => new
                {
                    Total = g.Sum(r => r.Amount),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (stats == null)
                return (0, 0);

            return (stats.Total, stats.Count);
        }
    }
}
