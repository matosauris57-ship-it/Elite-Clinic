namespace Clinic_System.Application.Service.Interface;

public interface IToothChartService
{
    Task<ToothChartEntry> CreateEntryAsync(
        int patientId,
        int toothNumber,
        ToothSurface surface,
        ToothChartPhase phase,
        ToothCondition condition,
        ToothSeverity? severity,
        string? notes,
        int? appointmentId,
        string? recordedByUserId,
        RestorationMaterial? restorationMaterial = null,
        CariesType? cariesType = null,
        IcdasCode? icdas = null,
        string? clinicalDiagnosis = null,
        string? proposedTreatment = null,
        Guid? bridgeSpanId = null,
        BridgeRole? bridgeRole = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ToothChartEntry>> GetCurrentAsync(
        int patientId,
        string? dentition,
        int? quadrant,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<DentalClinicalEvent>> GetTimelineAsync(
        int patientId,
        int? toothNumber,
        CancellationToken cancellationToken = default);

    Task<ToothChartEntry?> GetEntryAsync(long id, CancellationToken cancellationToken = default);

    Task<ToothChartEntry> UpdateEntryAsync(
        long id,
        ToothSurface surface,
        ToothChartPhase phase,
        ToothCondition condition,
        ToothSeverity? severity,
        string? notes,
        RestorationMaterial? restorationMaterial,
        CariesType? cariesType,
        IcdasCode? icdas,
        string? clinicalDiagnosis,
        string? proposedTreatment,
        CancellationToken cancellationToken = default);

    Task VoidEntryAsync(
        long id,
        string? voidedByUserId,
        string? reason = null,
        CancellationToken cancellationToken = default);
}
