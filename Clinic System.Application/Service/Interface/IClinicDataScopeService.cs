namespace Clinic_System.Application.Service.Interface;

public interface IClinicDataScopeService
{
    bool RestrictsToOwnDoctorData { get; }
    int? ScopedDoctorId { get; }

    int? ResolveListDoctorId(int? requestedDoctorId);
    bool AllowsDoctor(int doctorId);
    Task<bool> AllowsPatientAsync(int patientId, CancellationToken cancellationToken = default);
}
