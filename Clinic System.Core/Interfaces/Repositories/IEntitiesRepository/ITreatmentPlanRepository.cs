namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface ITreatmentPlanRepository : IGenericRepository<TreatmentPlan>
    {
        Task<TreatmentPlan?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TreatmentPlan>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    }
}
