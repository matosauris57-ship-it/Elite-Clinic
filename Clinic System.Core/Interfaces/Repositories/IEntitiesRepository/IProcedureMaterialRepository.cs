namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IProcedureMaterialRepository : IGenericRepository<ProcedureMaterial>
    {
        Task<List<ProcedureMaterial>> GetByProcedureIdAsync(int treatmentProcedureId, CancellationToken cancellationToken = default);
        Task<List<ProcedureMaterial>> GetByProcedureIdWithItemsAsync(int treatmentProcedureId, CancellationToken cancellationToken = default);
    }
}
