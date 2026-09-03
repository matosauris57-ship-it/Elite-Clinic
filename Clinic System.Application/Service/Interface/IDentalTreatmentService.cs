namespace Clinic_System.Application.Service.Interface
{
    public interface IDentalTreatmentService
    {
        Task<DentalTreatment> CreateAsync(
            int patientId,
            string procedureName,
            decimal cost,
            int? appointmentId,
            int? toothNumber,
            ToothSurface? toothSurface,
            int? treatmentProcedureId,
            string? procedureDetails,
            string? medicalNotes,
            string? recordedByUserId,
            CancellationToken cancellationToken = default);

        Task<DentalTreatment> StartAsync(int treatmentId, string? recordedByUserId, CancellationToken cancellationToken = default);
        Task<DentalTreatment> CompleteAsync(int treatmentId, string? recordedByUserId, CancellationToken cancellationToken = default);
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
        Task<DentalTreatment> GetByIdAsync(int treatmentId, CancellationToken cancellationToken = default);
        Task<DentalTreatment> UpdateAsync(
            int treatmentId,
            string procedureName,
            decimal cost,
            int? toothNumber,
            ToothSurface? toothSurface,
            int? treatmentProcedureId,
            string? procedureDetails,
            string? medicalNotes,
            CancellationToken cancellationToken = default);
        Task<DentalTreatment> CancelAsync(int treatmentId, string? reason, string? recordedByUserId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int treatmentId, CancellationToken cancellationToken = default);
    }
}
