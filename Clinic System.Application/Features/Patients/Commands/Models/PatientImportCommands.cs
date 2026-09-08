using Clinic_System.Application.DTOs.Patients;
using MediatR;

namespace Clinic_System.Application.Features.Patients.Commands.Models;

public class PreviewPatientImportCommand : IRequest<Response<PatientImportPreviewDTO>>
{
    public string CsvContent { get; set; } = string.Empty;
}

public class ConfirmPatientImportCommand : IRequest<Response<PatientImportResultDTO>>
{
    public List<PatientImportConfirmRow> Rows { get; set; } = [];
}
