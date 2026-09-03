namespace Clinic_System.Application.Service.Implemention
{
    public class MedicalConditionService : IMedicalConditionService
    {
        private readonly IUnitOfWork unitOfWork;

        public MedicalConditionService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public Task<IEnumerable<MedicalCondition>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
            => unitOfWork.MedicalConditionsRepository.GetAllAsync(activeOnly, cancellationToken);

        public async Task<MedicalCondition> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var condition = await unitOfWork.MedicalConditionsRepository.GetByIdAsync(id, cancellationToken);
            if (condition == null)
                throw new NotFoundException($"Medical condition with ID {id} not found.");
            return condition;
        }

        public async Task<MedicalCondition> CreateAsync(
            string name,
            string? category,
            bool isActive,
            int sortOrder,
            CancellationToken cancellationToken = default)
        {
            var trimmedName = name.Trim();
            var existing = await unitOfWork.MedicalConditionsRepository.GetByNameAsync(trimmedName, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException($"Ya existe una enfermedad con el nombre '{trimmedName}'.");

            var condition = new MedicalCondition
            {
                Name = trimmedName,
                Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
                IsActive = isActive,
                SortOrder = sortOrder
            };

            await unitOfWork.MedicalConditionsRepository.AddAsync(condition, cancellationToken);
            return condition;
        }

        public async Task<MedicalCondition> UpdateAsync(
            int id,
            string name,
            string? category,
            bool isActive,
            int sortOrder,
            CancellationToken cancellationToken = default)
        {
            var condition = await GetByIdAsync(id, cancellationToken);
            var trimmedName = name.Trim();
            var existing = await unitOfWork.MedicalConditionsRepository.GetByNameAsync(trimmedName, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException($"Ya existe una enfermedad con el nombre '{trimmedName}'.");

            condition.Name = trimmedName;
            condition.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            condition.IsActive = isActive;
            condition.SortOrder = sortOrder;

            unitOfWork.MedicalConditionsRepository.Update(condition, cancellationToken);
            return condition;
        }

        public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var condition = await GetByIdAsync(id, cancellationToken);
            condition.IsDeleted = true;
            condition.DeletedAt = DateTime.Now;
            unitOfWork.MedicalConditionsRepository.Update(condition, cancellationToken);
        }
    }
}
