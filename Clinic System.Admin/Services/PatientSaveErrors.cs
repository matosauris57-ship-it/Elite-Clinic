using Clinic_System.Core.Validation;
using DentalCare.Admin.Models;

namespace DentalCare.Admin.Services;

public static class PatientSaveErrors
{
    public static (string Error, int? DuplicatePatientId) FromApi<T>(ApiResponse<T>? body, string fallback)
    {
        var duplicateId = PatientFieldLimits.TryParseExistingPatientId(body?.Errors, body?.Message, out var id)
            ? id
            : (int?)null;

        string error;
        if (duplicateId.HasValue)
            error = "Esta cédula ya existe";
        else if (body?.Errors is { Count: > 0 })
            error = string.Join(" · ", body.Errors.Select(Translate));
        else
            error = Translate(body?.Message) ?? fallback;

        return (error, duplicateId);
    }

    public static int? FindLocalDuplicate(IEnumerable<PatientListItem> patients, string? nationalId, int? excludeId = null)
    {
        var key = PatientFieldLimits.NormalizeNationalId(nationalId);
        if (key.Length == 0)
            return null;

        return patients.FirstOrDefault(p =>
            p.Id != excludeId
            && PatientFieldLimits.NormalizeNationalId(p.NationalId) == key)?.Id;
    }

    public static string Clamp(string? value, int max, out bool truncated)
    {
        var text = value ?? string.Empty;
        truncated = text.Length > max;
        return truncated ? text[..max] : text;
    }

    private static string Translate(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return "No se pudo completar la operación.";

        return error switch
        {
            var e when e.Contains("Name must not contain numbers", StringComparison.OrdinalIgnoreCase) =>
                "El nombre no puede contener números.",
            var e when e.Contains("National ID format", StringComparison.OrdinalIgnoreCase) =>
                "La cédula solo puede incluir números y guiones.",
            var e when e.Contains("Phone number is already", StringComparison.OrdinalIgnoreCase) =>
                "Este teléfono ya está registrado.",
            var e when e.Contains("must not exceed", StringComparison.OrdinalIgnoreCase) =>
                "Uno de los campos supera el tamaño permitido.",
            var e when e.Contains("Phone number must contain", StringComparison.OrdinalIgnoreCase) =>
                $"El teléfono debe tener entre {PatientFieldLimits.PhoneMinDigits} y {PatientFieldLimits.PhoneMaxDigits} dígitos.",
            var e when e.Contains("Validation Failed", StringComparison.OrdinalIgnoreCase) =>
                "Revise los datos del paciente.",
            _ => error
        };
    }
}
