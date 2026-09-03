namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class DentalTreatmentRepository : GenericRepository<DentalTreatment>, IDentalTreatmentRepository
    {
        public DentalTreatmentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<DentalTreatment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await context.DentalTreatments
                .AsNoTracking()
                .Include(t => t.Appointment)
                .Include(t => t.ToothRecord)
                .Where(t => t.PatientId == patientId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DentalTreatment>> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            return await context.DentalTreatments
                .AsNoTracking()
                .Where(t => t.AppointmentId == appointmentId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<DentalTreatment> Items, int TotalCount, Dictionary<DentalTreatmentStatus, int> StatusCounts)> GetAllForAdminAsync(
            string? search,
            IReadOnlyCollection<DentalTreatmentStatus>? statuses,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = context.DentalTreatments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t =>
                    t.ProcedureName.Contains(term) ||
                    t.Patient.FullName.Contains(term));
            }

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(t => (t.PerformedDate ?? t.CreatedAt) >= start);
            }

            if (toDate.HasValue)
            {
                var endExclusive = toDate.Value.Date.AddDays(1);
                query = query.Where(t => (t.PerformedDate ?? t.CreatedAt) < endExclusive);
            }

            var statusCounts = await query
                .GroupBy(t => t.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

            if (statuses is { Count: > 0 })
            {
                var statusList = statuses.ToArray();
                query = query.Where(t => statusList.Contains(t.Status));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = query
                .OrderByDescending(t => t.PerformedDate ?? t.CreatedAt)
                .Include(t => t.Patient);

            if (pageSize > 0)
            {
                if (pageNumber < 1)
                    pageNumber = 1;

                query = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }

            var items = await query.ToListAsync(cancellationToken);
            return (items, totalCount, statusCounts);
        }

        public async Task<DentalTreatment?> GetByIdWithPatientAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.DentalTreatments
                .Include(t => t.Patient)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }
    }
}
