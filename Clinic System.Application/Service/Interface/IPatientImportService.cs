using Clinic_System.Application.DTOs.Patients;

namespace Clinic_System.Application.Service.Interface;

public interface IPatientImportService
{
    Task<PatientImportPreviewDTO> PreviewAsync(string csvContent, CancellationToken cancellationToken = default);
    Task<PatientImportResultDTO> ImportAsync(
        IReadOnlyList<PatientImportConfirmRow> rows,
        CancellationToken cancellationToken = default);
}
