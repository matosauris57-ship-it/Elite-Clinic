namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class PatientMedicalCertificateRepository : GenericRepository<PatientMedicalCertificate>, IPatientMedicalCertificateRepository
{
    public PatientMedicalCertificateRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PatientMedicalCertificate>> GetByPatientAsync(
        int patientId,
        CancellationToken cancellationToken = default) =>
        await context.PatientMedicalCertificates
            .AsNoTracking()
            .Include(x => x.Doctor)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.IssuedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<PatientMedicalCertificate?> GetWithDetailsAsync(
        int certificateId,
        CancellationToken cancellationToken = default) =>
        await context.PatientMedicalCertificates
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == certificateId, cancellationToken);
}
