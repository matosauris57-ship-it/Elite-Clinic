namespace Clinic_System.Application.Service.Interface;

public interface IPatientMedicalCertificateService
{
    IReadOnlyList<MedicalCertificateOptionDTO> ListCertificateTypes();
    IReadOnlyList<MedicalCertificateOptionDTO> ListPurposes();
    IReadOnlyList<string> ListDiagnosisSuggestions();
    Task<IReadOnlyList<PatientMedicalCertificate>> ListByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PatientMedicalCertificate> GetAsync(int certificateId, CancellationToken cancellationToken = default);
    Task<PatientMedicalCertificate> CreateAsync(int patientId, PatientMedicalCertificateUpsertDTO request, int? doctorId, string? recordedByUserId, CancellationToken cancellationToken = default);
    Task<PatientMedicalCertificate> UpdateAsync(int certificateId, PatientMedicalCertificateUpsertDTO request, int? doctorId, string? recordedByUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int certificateId, CancellationToken cancellationToken = default);
}
