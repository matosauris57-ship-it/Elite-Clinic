namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IPatientMedicalConditionRepository
    {
        Task<IReadOnlyList<PatientMedicalCondition>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
        Task SyncPatientConditionsAsync(int patientId, IEnumerable<int> conditionIds, CancellationToken cancellationToken = default);
    }
}
