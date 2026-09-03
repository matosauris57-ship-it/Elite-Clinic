namespace Clinic_System.Data.Repository.RepositoriesForEntities;

public class ToothChartEntryRepository : GenericRepository<ToothChartEntry>, IToothChartEntryRepository
{
    public ToothChartEntryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ToothChartEntry>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default) =>
        await context.ToothChartEntries
            .AsNoTracking()
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.RecordedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
}

public class DentalClinicalEventRepository : GenericRepository<DentalClinicalEvent>, IDentalClinicalEventRepository
{
    public DentalClinicalEventRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<DentalClinicalEvent>> GetTimelineAsync(int patientId, int? toothNumber, CancellationToken cancellationToken = default) =>
        await context.DentalClinicalEvents
            .AsNoTracking()
            .Where(x => x.PatientId == patientId && (!toothNumber.HasValue || x.ToothNumber == toothNumber))
            .OrderByDescending(x => x.RecordedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<DentalClinicalEvent>> GetRecentAsync(DateTime since, int take, CancellationToken cancellationToken = default) =>
        await context.DentalClinicalEvents
            .AsNoTracking()
            .Include(x => x.Patient)
            .Where(x => x.RecordedAt >= since)
            .OrderByDescending(x => x.RecordedAt)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
}
