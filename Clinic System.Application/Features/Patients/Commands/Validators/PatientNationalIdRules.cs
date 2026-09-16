namespace Clinic_System.Application.Features.Patients.Commands.Validators;

internal static class PatientNationalIdRules
{
    public static async Task<Patient?> FindDuplicateAsync(IUnitOfWork unitOfWork, string? nationalId, int? excludePatientId = null)
    {
        var key = PatientFieldLimits.NormalizeNationalId(nationalId);
        if (key.Length == 0)
            return null;

        var patients = await unitOfWork.PatientsRepository.FindAsync(p => p.NationalId != null);
        return patients.FirstOrDefault(p =>
            (!excludePatientId.HasValue || p.Id != excludePatientId.Value)
            && PatientFieldLimits.NormalizeNationalId(p.NationalId) == key);
    }
}
