namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface ITreatmentMaterialConsumptionRepository : IGenericRepository<TreatmentMaterialConsumption>
    {
        Task<TreatmentMaterialConsumption?> GetByTreatmentIdAsync(int dentalTreatmentId, CancellationToken cancellationToken = default);
        Task<TreatmentMaterialConsumption?> GetByTreatmentIdWithLinesAsync(int dentalTreatmentId, CancellationToken cancellationToken = default);
        Task<TreatmentMaterialConsumption?> GetByTreatmentIdForUpdateAsync(int dentalTreatmentId, CancellationToken cancellationToken = default);
    }
}
