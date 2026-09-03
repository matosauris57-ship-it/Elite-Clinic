namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IMedicalConditionRepository : IGenericRepository<MedicalCondition>
    {
        Task<IEnumerable<MedicalCondition>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
        Task<MedicalCondition?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
