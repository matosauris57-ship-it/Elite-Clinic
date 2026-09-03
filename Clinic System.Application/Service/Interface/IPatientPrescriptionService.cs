namespace Clinic_System.Application.Service.Interface;

public interface IPatientPrescriptionService
{
    IReadOnlyList<PrescriptionTemplateDTO> ListTemplates();
    Task<IReadOnlyList<PatientPrescription>> ListByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PatientPrescription> GetAsync(int prescriptionId, CancellationToken cancellationToken = default);
    Task<PatientPrescription> CreateAsync(int patientId, PatientPrescriptionUpsertDTO request, int? doctorId, string? recordedByUserId, CancellationToken cancellationToken = default);
    Task<PatientPrescription> UpdateAsync(int prescriptionId, PatientPrescriptionUpsertDTO request, int? doctorId, string? recordedByUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int prescriptionId, CancellationToken cancellationToken = default);
}
