namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IDoctorProcedurePriceRepository : IGenericRepository<DoctorProcedurePrice>
    {
        Task<IReadOnlyList<DoctorProcedurePrice>> GetByProcedureIdAsync(int procedureId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DoctorProcedurePrice>> GetByProcedureIdsAsync(IEnumerable<int> procedureIds, CancellationToken cancellationToken = default);
        Task<DoctorProcedurePrice?> GetAsync(int doctorId, int procedureId, CancellationToken cancellationToken = default);
        void RemoveRange(IEnumerable<DoctorProcedurePrice> prices);
    }
}
