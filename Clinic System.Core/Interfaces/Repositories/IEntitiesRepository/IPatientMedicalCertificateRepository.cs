namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IPatientMedicalCertificateRepository : IGenericRepository<PatientMedicalCertificate>
{
    Task<IReadOnlyList<PatientMedicalCertificate>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PatientMedicalCertificate?> GetWithDetailsAsync(int certificateId, CancellationToken cancellationToken = default);
}
