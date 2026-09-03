namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class PatientPrescriptionRepository : GenericRepository<PatientPrescription>, IPatientPrescriptionRepository
{
    public PatientPrescriptionRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PatientPrescription>> GetByPatientAsync(
        int patientId,
        CancellationToken cancellationToken = default) =>
        await context.PatientPrescriptions
            .AsNoTracking()
            .Include(x => x.Doctor)
            .Include(x => x.Items)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.IssuedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<PatientPrescription?> GetWithItemsAsync(int prescriptionId, CancellationToken cancellationToken = default) =>
        await context.PatientPrescriptions
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == prescriptionId, cancellationToken);

    public void RemoveItem(PatientPrescriptionItem item) => context.Set<PatientPrescriptionItem>().Remove(item);
}
