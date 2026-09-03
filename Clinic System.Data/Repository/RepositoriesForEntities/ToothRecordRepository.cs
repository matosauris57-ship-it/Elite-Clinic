namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class ToothRecordRepository : GenericRepository<ToothRecord>, IToothRecordRepository
    {
        public ToothRecordRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ToothRecord>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await context.ToothRecords
                .AsNoTracking()
                .Where(t => t.PatientId == patientId)
                .OrderBy(t => t.ToothNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<ToothRecord?> GetByPatientAndToothAsync(int patientId, int toothNumber, CancellationToken cancellationToken = default)
        {
            return await context.ToothRecords
                .FirstOrDefaultAsync(t => t.PatientId == patientId && t.ToothNumber == toothNumber, cancellationToken);
        }
    }
}
