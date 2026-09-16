namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class PatientRepository : GenericRepository<Patient>, IPatientRepository
    {
        public PatientRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Patient?> GetPatientByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ApplicationUserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Patient?>> GetPatientsWithAppointmentsAsync(Expression<Func<Appointment, bool>> appointmentPredicate)
        {
            // الحل: استخدام Join مع Appointments مباشرة بدلاً من AsQueryable()
            // هذا يضمن تنفيذ Query في SQL وليس في Memory
            var patientIds = context.Appointments
                .Where(appointmentPredicate)
                .Select(a => a.PatientId)
                .Distinct();

            return await context.Patients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientWithAppointmentsByIdAsync(int Id, CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .AsNoTracking()
                .Include(d => d.Appointments.OrderBy(a => a.AppointmentDate))
                .FirstOrDefaultAsync(d => d.Id == Id);
        }

        public async Task<IEnumerable<Patient?>> GetPatientsByNameAsync(string fullName, CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .AsNoTracking()
                .Where(d => EF.Functions.Like(d.FullName, $"%{fullName}%"))
                .OrderBy(d => d.FullName)
                .ToListAsync(cancellationToken);
        }

        public async Task<Patient?> GetPatientByPhoneAsync(string Phone, CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Phone == Phone, cancellationToken);
        }

        public async Task<string?> GetPatientUserIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .AsNoTracking()
                .Where(p => p.Id == patientId)
                .Select(p => p.ApplicationUserId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<IEnumerable<Patient?>> GetAllForAdminAsync(bool includeInactive, CancellationToken cancellationToken = default)
            => GetAllForAdminAsync(includeInactive, attendedByDoctorId: null, cancellationToken);

        public async Task<IEnumerable<Patient?>> GetAllForAdminAsync(
            bool includeInactive,
            int? attendedByDoctorId,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Patient> query = context.Patients.AsNoTracking();

            if (includeInactive)
                query = query.IgnoreQueryFilters();

            if (attendedByDoctorId.HasValue)
            {
                var doctorId = attendedByDoctorId.Value;
                var relatedIds = context.Appointments
                    .Where(a => a.DoctorId == doctorId)
                    .Select(a => a.PatientId)
                    .Distinct();

                query = query.Where(p => relatedIds.Contains(p.Id));
            }

            return await query
                .OrderBy(p => p.IsDeleted)
                .ThenBy(p => p.FullName)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Patient> Items, int TotalCount)> GetFilteredForAdminPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            string status,
            int? attendedByDoctorId = null,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);
            status = string.IsNullOrWhiteSpace(status) ? "all" : status.Trim().ToLowerInvariant();

            IQueryable<Patient> query = context.Patients.AsNoTracking();

            if (status is "all" or "inactive")
                query = query.IgnoreQueryFilters();

            query = status switch
            {
                "active" => query.Where(p => !p.IsDeleted),
                "inactive" => query.Where(p => p.IsDeleted),
                _ => query
            };

            if (attendedByDoctorId.HasValue)
            {
                var doctorId = attendedByDoctorId.Value;
                var relatedIds = context.Appointments
                    .Where(a => a.DoctorId == doctorId)
                    .Select(a => a.PatientId)
                    .Distinct();
                query = query.Where(p => relatedIds.Contains(p.Id));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.FullName, $"%{term}%")
                    || (p.NationalId != null && EF.Functions.Like(p.NationalId, $"%{term}%"))
                    || (p.Phone != null && EF.Functions.Like(p.Phone, $"%{term}%"))
                    || (p.MobilePhone != null && EF.Functions.Like(p.MobilePhone, $"%{term}%"))
                    || (p.Email != null && EF.Functions.Like(p.Email, $"%{term}%")));
            }

            query = query
                .OrderBy(p => p.IsDeleted)
                .ThenBy(p => p.FullName);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> IsLinkedToDoctorAsync(int patientId, int doctorId, CancellationToken cancellationToken = default)
        {
            return await context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorId, cancellationToken);
        }

        public async Task<Patient?> GetByIdIncludingDeletedAsync(int id, CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<List<Patient>> GetForBirthdayEmailsAsync(int year, CancellationToken cancellationToken = default)
        {
            return await context.Patients
                .Where(p => p.Email != null
                    && p.Email != ""
                    && !p.EmailInvalid
                    && (p.BirthdayEmailLastSentYear == null || p.BirthdayEmailLastSentYear != year))
                .ToListAsync(cancellationToken);
        }

        public Task<List<Patient>> GetEmailCampaignAudienceAsync(CancellationToken cancellationToken = default) =>
            context.Patients
                .Where(p => p.Email != null
                    && p.Email != ""
                    && !p.OptOutEmailCampaigns
                    && !p.EmailInvalid)
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);

        public async Task<(int WithEmail, int OptedOut, int Invalid, int Eligible)> CountEmailCampaignAudienceAsync(
            CancellationToken cancellationToken = default)
        {
            var withEmail = await context.Patients.CountAsync(
                p => p.Email != null && p.Email != "", cancellationToken);
            var optedOut = await context.Patients.CountAsync(
                p => p.Email != null && p.Email != "" && p.OptOutEmailCampaigns, cancellationToken);
            var invalid = await context.Patients.CountAsync(
                p => p.Email != null && p.Email != "" && p.EmailInvalid, cancellationToken);
            var eligible = await context.Patients.CountAsync(
                p => p.Email != null && p.Email != "" && !p.OptOutEmailCampaigns && !p.EmailInvalid,
                cancellationToken);
            return (withEmail, optedOut, invalid, eligible);
        }

        public async Task<(HashSet<string> Phones, HashSet<string> NationalIds)> GetImportIdentityKeysAsync(
            CancellationToken cancellationToken = default)
        {
            var phones = await context.Patients
                .AsNoTracking()
                .Select(p => p.Phone)
                .ToListAsync(cancellationToken);

            var nationalIds = await context.Patients
                .AsNoTracking()
                .Where(p => p.NationalId != null && p.NationalId != "")
                .Select(p => p.NationalId!)
                .ToListAsync(cancellationToken);

            return (
                new HashSet<string>(phones, StringComparer.OrdinalIgnoreCase),
                new HashSet<string>(nationalIds, StringComparer.OrdinalIgnoreCase));
        }
    }
}
