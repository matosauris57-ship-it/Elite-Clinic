namespace Clinic_System.Application.Service.Interface
{
    public interface IToothRecordService
    {
        Task<ToothRecord> UpsertDiagnosisAsync(int patientId, int toothNumber, ToothCondition condition, string? notes, CancellationToken cancellationToken = default);
        Task<ToothRecord> UpsertTreatmentAsync(int patientId, int toothNumber, ToothCondition? condition, CancellationToken cancellationToken = default);
        Task BatchUpsertAsync(int patientId, IEnumerable<OdontogramToothInput> teeth, CancellationToken cancellationToken = default);
        Task<IEnumerable<ToothRecord>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    }

    public record OdontogramToothInput(int ToothNumber, ToothCondition DiagnosisCondition, ToothCondition? TreatmentCondition, string? Notes);
}
