namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IToothChartEntryRepository : IGenericRepository<ToothChartEntry>
{
    Task<IEnumerable<ToothChartEntry>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);
}

public interface IDentalClinicalEventRepository : IGenericRepository<DentalClinicalEvent>
{
    Task<IEnumerable<DentalClinicalEvent>> GetTimelineAsync(int patientId, int? toothNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<DentalClinicalEvent>> GetRecentAsync(DateTime since, int take, CancellationToken cancellationToken = default);
}
