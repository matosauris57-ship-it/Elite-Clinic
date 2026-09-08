using Clinic_System.Application.DTOs.Patients;
using Clinic_System.Application.Features.Patients.Commands.Models;
using Clinic_System.Application.Service.Interface;
using FluentValidation;

namespace Clinic_System.Application.Features.Patients.Commands.Handlers;

public class PreviewPatientImportCommandHandler
    : ResponseHandler, IRequestHandler<PreviewPatientImportCommand, Response<PatientImportPreviewDTO>>
{
    private readonly IPatientImportService _importService;

    public PreviewPatientImportCommandHandler(IPatientImportService importService)
    {
        _importService = importService;
    }

    public async Task<Response<PatientImportPreviewDTO>> Handle(
        PreviewPatientImportCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CsvContent))
            return BadRequest<PatientImportPreviewDTO>("Suba un archivo CSV.");

        var preview = await _importService.PreviewAsync(request.CsvContent, cancellationToken);
        return Success(preview);
    }
}

public class ConfirmPatientImportCommandHandler
    : ResponseHandler, IRequestHandler<ConfirmPatientImportCommand, Response<PatientImportResultDTO>>
{
    private readonly IPatientImportService _importService;
    private readonly ILogger<ConfirmPatientImportCommandHandler> _logger;

    public ConfirmPatientImportCommandHandler(
        IPatientImportService importService,
        ILogger<ConfirmPatientImportCommandHandler> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    public async Task<Response<PatientImportResultDTO>> Handle(
        ConfirmPatientImportCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _importService.ImportAsync(request.Rows ?? [], cancellationToken);
            _logger.LogInformation(
                "Importación de pacientes: {Imported} creados, {Skipped} omitidos de {Requested}",
                result.Imported, result.Skipped, result.Requested);
            return Success(result, $"Se importaron {result.Imported} paciente(s).");
        }
        catch (ValidationException ex)
        {
            return BadRequest<PatientImportResultDTO>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar pacientes");
            return BadRequest<PatientImportResultDTO>($"No se pudo completar la importación: {ex.Message}");
        }
    }
}
