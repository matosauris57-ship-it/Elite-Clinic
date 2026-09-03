namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class PeriodontalExamRepository : GenericRepository<PeriodontalExam>, IPeriodontalExamRepository
{
    public PeriodontalExamRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PeriodontalExam>> GetSummariesByPatientAsync(
        int patientId,
        CancellationToken cancellationToken = default) =>
        await context.PeriodontalExams
            .AsNoTracking()
            .Include(x => x.Doctor)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.ExaminedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<PeriodontalExam?> GetWithChartAsync(int examId, CancellationToken cancellationToken = default) =>
        await context.PeriodontalExams
            .Include(x => x.Teeth)
                .ThenInclude(x => x.Sites)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);

    public async Task<PeriodontalExam?> GetLatestByPatientAsync(int patientId, CancellationToken cancellationToken = default) =>
        await context.PeriodontalExams
            .Include(x => x.Teeth)
                .ThenInclude(x => x.Sites)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.ExaminedAt)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public void RemoveTooth(PeriodontalTooth tooth) => context.PeriodontalTeeth.Remove(tooth);
}
