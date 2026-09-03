namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository
{
    public interface IDentalTreatmentRepository : IGenericRepository<DentalTreatment>
    {
        Task<IEnumerable<DentalTreatment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<DentalTreatment>> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<(List<DentalTreatment> Items, int TotalCount, Dictionary<DentalTreatmentStatus, int> StatusCounts)> GetAllForAdminAsync(
            string? search,
            IReadOnlyCollection<DentalTreatmentStatus>? statuses,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<DentalTreatment?> GetByIdWithPatientAsync(int id, CancellationToken cancellationToken = default);
    }
}