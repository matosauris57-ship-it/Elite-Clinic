namespace Clinic_System.Application.Service.Interface
{
    public interface IMedicalConditionService
    {
        Task<IEnumerable<MedicalCondition>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
        Task<MedicalCondition> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<MedicalCondition> CreateAsync(string name, string? category, bool isActive, int sortOrder, CancellationToken cancellationToken = default);
        Task<MedicalCondition> UpdateAsync(int id, string name, string? category, bool isActive, int sortOrder, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
