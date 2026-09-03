namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface ITreatmentProcedureRepository : IGenericRepository<TreatmentProcedure>
    {
        Task<IEnumerable<TreatmentProcedure>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
        Task<TreatmentProcedure?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
