namespace Clinic_System.Application.Service.Interface
{
    public interface ITreatmentProcedureService
    {
        Task<IEnumerable<TreatmentProcedure>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);
        Task<TreatmentProcedure> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<TreatmentProcedure> CreateAsync(
            string code,
            string category,
            string name,
            decimal price,
            int durationMinutes,
            bool isActive,
            CancellationToken cancellationToken = default);
        Task<TreatmentProcedure> UpdateAsync(
            int id,
            string code,
            string category,
            string name,
            decimal price,
            int durationMinutes,
            bool isActive,
            CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task ReplaceDoctorPricesAsync(int procedureId, IEnumerable<DoctorProcedurePriceInput> prices, CancellationToken cancellationToken = default);
        Task<decimal> ResolvePriceAsync(int procedureId, int? doctorId, CancellationToken cancellationToken = default);
        Task<TreatmentProcedureDTO> ToDtoAsync(TreatmentProcedure procedure, int? doctorId, CancellationToken cancellationToken = default);
        Task<List<TreatmentProcedureDTO>> ToDtosAsync(IEnumerable<TreatmentProcedure> procedures, int? doctorId, CancellationToken cancellationToken = default);
    }
}
