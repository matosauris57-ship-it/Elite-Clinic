namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IToothRecordRepository : IGenericRepository<ToothRecord>
    {
        Task<IEnumerable<ToothRecord>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
        Task<ToothRecord?> GetByPatientAndToothAsync(int patientId, int toothNumber, CancellationToken cancellationToken = default);
    }
}
