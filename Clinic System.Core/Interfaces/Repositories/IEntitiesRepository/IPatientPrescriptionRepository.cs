namespace Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;

public interface IPatientPrescriptionRepository : IGenericRepository<PatientPrescription>
{
    Task<IReadOnlyList<PatientPrescription>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PatientPrescription?> GetWithItemsAsync(int prescriptionId, CancellationToken cancellationToken = default);
    void RemoveItem(PatientPrescriptionItem item);
}
