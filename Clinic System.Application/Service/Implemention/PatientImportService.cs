using Clinic_System.Application.DTOs.Patients;
using Clinic_System.Application.Service.Interface;
using Clinic_System.Core.Entities;
using Clinic_System.Core.Interfaces.UnitOfWork;
using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Service.Implemention;

public class PatientImportService : IPatientImportService
{
    private readonly IUnitOfWork _unitOfWork;

    public PatientImportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PatientImportPreviewDTO> PreviewAsync(string csvContent, CancellationToken cancellationToken = default)
    {
        var rows = PatientCsvImport.Parse(csvContent, out var parseError);
        if (parseError != null)
        {
            return new PatientImportPreviewDTO
            {
                TotalRows = 0,
                ValidCount = 0,
                ErrorCount = 1,
                Rows =
                [
                    new PatientImportRowDTO
                    {
                        RowNumber = 1,
                        IsValid = false,
                        Error = parseError
                    }
                ]
            };
        }

        var (existingPhones, existingNationalIds) = await LoadKeysAsync(cancellationToken);
        var previewRows = new List<PatientImportRowDTO>(rows.Count);

        foreach (var row in rows)
        {
            var error = PatientCsvImport.ValidateRow(row, existingPhones, existingNationalIds, out var gender, out var email);
            previewRows.Add(ToDto(row, gender.ToString(), email, error));
        }

        return new PatientImportPreviewDTO
        {
            TotalRows = previewRows.Count,
            ValidCount = previewRows.Count(r => r.IsValid),
            ErrorCount = previewRows.Count(r => !r.IsValid),
            Rows = previewRows
        };
    }

    public async Task<PatientImportResultDTO> ImportAsync(
        IReadOnlyList<PatientImportConfirmRow> rows,
        CancellationToken cancellationToken = default)
    {
        if (rows.Count == 0)
            throw new ValidationException("No hay filas para importar.");
        if (rows.Count > PatientCsvImport.MaxRows)
            throw new ValidationException($"No se pueden importar más de {PatientCsvImport.MaxRows} pacientes a la vez.");

        var (existingPhones, existingNationalIds) = await LoadKeysAsync(cancellationToken);
        var resultRows = new List<PatientImportRowDTO>(rows.Count);
        var toInsert = new List<(PatientImportConfirmRow Source, Patient Entity, string Gender, string? Email)>();

        foreach (var row in rows)
        {
            row.Phone = PatientCsvImport.NormalizePhone(row.Phone);
            row.MobilePhone = string.IsNullOrWhiteSpace(row.MobilePhone)
                ? null
                : PatientCsvImport.NormalizePhone(row.MobilePhone);

            var error = PatientCsvImport.ValidateRow(row, existingPhones, existingNationalIds, out var gender, out var email);
            if (error != null)
            {
                resultRows.Add(ToDto(row, gender.ToString(), email, error));
                continue;
            }

            var entity = new Patient
            {
                FullName = row.FullName.Trim(),
                Gender = gender,
                DateOfBirth = row.DateOfBirth.Date,
                Address = string.IsNullOrWhiteSpace(row.Address) ? "Sin dirección" : row.Address.Trim(),
                Phone = row.Phone,
                MobilePhone = string.IsNullOrWhiteSpace(row.MobilePhone) ? null : row.MobilePhone,
                NationalId = string.IsNullOrWhiteSpace(row.NationalId) ? null : row.NationalId.Trim(),
                Email = ContactEmail.NormalizeOrNull(email),
                ApplicationUserId = null,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            toInsert.Add((row, entity, gender.ToString(), email));
            resultRows.Add(ToDto(row, gender.ToString(), email, null));
        }

        foreach (var item in toInsert)
            await _unitOfWork.PatientsRepository.AddAsync(item.Entity, cancellationToken);

        if (toInsert.Count > 0)
        {
            var saved = await _unitOfWork.SaveAsync(cancellationToken);
            if (saved == 0)
                throw new InvalidOperationException("No se pudieron guardar los pacientes.");
        }

        for (var i = 0; i < resultRows.Count; i++)
        {
            if (!resultRows[i].IsValid)
                continue;

            var match = toInsert.FirstOrDefault(t => t.Source.RowNumber == resultRows[i].RowNumber);
            if (match.Entity != null)
            {
                resultRows[i].Imported = true;
                resultRows[i].PatientId = match.Entity.Id;
            }
        }

        return new PatientImportResultDTO
        {
            Requested = rows.Count,
            Imported = resultRows.Count(r => r.Imported),
            Skipped = resultRows.Count(r => !r.IsValid),
            Rows = resultRows
        };
    }

    private async Task<(HashSet<string> Phones, HashSet<string> NationalIds)> LoadKeysAsync(
        CancellationToken cancellationToken)
    {
        var (patientPhones, nationalIds) =
            await _unitOfWork.PatientsRepository.GetImportIdentityKeysAsync(cancellationToken);
        var doctorPhones = await _unitOfWork.DoctorsRepository.GetAllPhonesAsync(cancellationToken);
        foreach (var phone in doctorPhones)
            patientPhones.Add(phone);
        return (patientPhones, nationalIds);
    }

    private static PatientImportRowDTO ToDto(
        PatientImportConfirmRow row,
        string gender,
        string? email,
        string? error) => new()
    {
        RowNumber = row.RowNumber,
        IsValid = error == null,
        Error = error,
        FullName = row.FullName?.Trim() ?? string.Empty,
        Gender = gender,
        DateOfBirth = row.DateOfBirth == default ? string.Empty : row.DateOfBirth.ToString("yyyy-MM-dd"),
        Address = row.Address?.Trim() ?? string.Empty,
        Phone = row.Phone ?? string.Empty,
        NationalId = string.IsNullOrWhiteSpace(row.NationalId) ? null : row.NationalId.Trim(),
        Email = email,
        MobilePhone = string.IsNullOrWhiteSpace(row.MobilePhone) ? null : row.MobilePhone
    };
}
