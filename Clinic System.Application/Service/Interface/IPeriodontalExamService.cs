namespace Clinic_System.Application.Service.Interface;

public interface IPeriodontalExamService
{
    Task<IReadOnlyList<PeriodontalExam>> ListByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PeriodontalExam> GetChartAsync(int examId, CancellationToken cancellationToken = default);
    Task<PeriodontalExam> CreateAsync(int patientId, bool copyLatest, int? doctorId, string? recordedByUserId, CancellationToken cancellationToken = default);
    Task<PeriodontalExam> SaveAsync(int examId, PeriodontalExamUpsertDTO chart, string? recordedByUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int examId, CancellationToken cancellationToken = default);
    Task<PeriodontalCompareDTO> CompareAsync(int previousExamId, int currentExamId, CancellationToken cancellationToken = default);
}
