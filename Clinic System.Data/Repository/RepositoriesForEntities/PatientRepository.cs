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

        public async Task<IEnumerable<Patient?>> GetAllForAdminAsync(bool includeInactive, CancellationToken cancellationToken = default)
        {
            IQueryable<Patient> query = context.Patients.AsNoTracking();

            if (includeInactive)
                query = query.IgnoreQueryFilters();

            return await query
                .OrderBy(p => p.FullName)
                .ToListAsync(cancellationToken);
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
    }
}
