namespace Clinic_System.Application.Service.Implemention;

public class ClinicDataScopeService : IClinicDataScopeService
{
    private readonly ICurrentUserService _currentUser;

    public ClinicDataScopeService(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public bool RestrictsToOwnDoctorData => _currentUser.RestrictsToOwnDoctorData;

    public int? ScopedDoctorId => RestrictsToOwnDoctorData ? _currentUser.DoctorId : null;

    public int? ResolveListDoctorId(int? requestedDoctorId)
    {
        if (RestrictsToOwnDoctorData)
            return _currentUser.DoctorId;

        return requestedDoctorId;
    }

    public bool AllowsDoctor(int doctorId) =>
        !RestrictsToOwnDoctorData || _currentUser.DoctorId == doctorId;

    public Task<bool> AllowsPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        // El directorio y la ficha de pacientes son de toda la clínica.
        // "Solo sus datos" limita agenda, citas de otros médicos y listados globales, no pacientes.
        return Task.FromResult(true);
    }
}
