namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IPeriodontalExamRepository : IGenericRepository<PeriodontalExam>
{
    Task<IReadOnlyList<PeriodontalExam>> GetSummariesByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PeriodontalExam?> GetWithChartAsync(int examId, CancellationToken cancellationToken = default);
    Task<PeriodontalExam?> GetLatestByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    void RemoveTooth(PeriodontalTooth tooth);
}
