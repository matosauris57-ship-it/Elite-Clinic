namespace Clinic_System.Data.Repository.RepositoriesForEntities
{
    public class PatientMedicalConditionRepository : IPatientMedicalConditionRepository
    {
        private readonly AppDbContext context;

        public PatientMedicalConditionRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IReadOnlyList<PatientMedicalCondition>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await context.PatientMedicalConditions
                .AsNoTracking()
                .Include(pc => pc.MedicalCondition)
                .Where(pc => pc.PatientId == patientId)
                .ToListAsync(cancellationToken);
        }

        public async Task SyncPatientConditionsAsync(int patientId, IEnumerable<int> conditionIds, CancellationToken cancellationToken = default)
        {
            var desired = conditionIds.Distinct().ToHashSet();
            var existing = await context.PatientMedicalConditions
                .Where(pc => pc.PatientId == patientId)
                .ToListAsync(cancellationToken);

            var toRemove = existing.Where(e => !desired.Contains(e.MedicalConditionId)).ToList();
            if (toRemove.Count > 0)
                context.PatientMedicalConditions.RemoveRange(toRemove);

            var existingIds = existing.Select(e => e.MedicalConditionId).ToHashSet();
            foreach (var conditionId in desired.Where(id => !existingIds.Contains(id)))
            {
                context.PatientMedicalConditions.Add(new PatientMedicalCondition
                {
                    PatientId = patientId,
                    MedicalConditionId = conditionId
                });
            }
        }
    }
}
