namespace Clinic_System.Core.Validation;

public static class PatientFieldLimits
{
    public const int FullName = 100;
    public const int Address = 200;
    public const int Phone = 20;
    public const int NationalId = 20;
    public const int Email = 120;
    public const int PhoneMinDigits = 10;
    public const int PhoneMaxDigits = 15;
    public const string ExistingPatientMarker = "existingPatientId=";

    public static string NormalizeNationalId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Where(char.IsLetterOrDigit).ToArray());
    }

    public static int CountDigits(string? value) =>
        string.IsNullOrEmpty(value) ? 0 : value.Count(char.IsDigit);

    public static string? ValidateLengths(string? fullName, string? nationalId, string? phone, string? email, string? address)
    {
        var nameError = PersonNameRules.Validate(fullName);
        if (nameError != null)
            return nameError;
        if (nationalId?.Length > NationalId)
            return $"La cédula admite máximo {NationalId} caracteres.";
        if (!IsValidNationalId(nationalId))
            return "La cédula solo puede incluir números y guiones (11 dígitos).";
        if (!IsValidPhone(phone, required: true))
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "El teléfono es obligatorio.";
            if (phone.Length > Phone)
                return $"El teléfono admite máximo {Phone} caracteres.";
            return $"El teléfono debe tener entre {PhoneMinDigits} y {PhoneMaxDigits} dígitos.";
        }
        if (email?.Length > Email)
            return $"El correo admite máximo {Email} caracteres.";
        if (address?.Length > Address)
            return $"La dirección admite máximo {Address} caracteres.";
        return null;
    }

    public static bool IsValidNationalId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (value.Any(c => !char.IsDigit(c) && c is not '-' and not ' '))
            return false;

        var digits = NormalizeNationalId(value);
        return digits.Length is >= 8 and <= 13 && digits.All(char.IsDigit);
    }

    public static bool IsValidPhone(string? phone, bool required = true)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return !required;

        if (phone.Length > Phone)
            return false;

        if (phone.Any(c => !char.IsDigit(c) && c is not '+' and not ' ' and not '-' and not '(' and not ')'))
            return false;

        var digits = CountDigits(phone);
        return digits is >= PhoneMinDigits and <= PhoneMaxDigits;
    }

    public static bool TryParseExistingPatientId(IEnumerable<string>? errors, string? message, out int patientId)
    {
        patientId = 0;
        foreach (var part in (errors ?? []).Append(message ?? string.Empty))
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            var idx = part.IndexOf(ExistingPatientMarker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                continue;

            var rest = part[(idx + ExistingPatientMarker.Length)..];
            var digits = new string(rest.TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var id) && id > 0)
            {
                patientId = id;
                return true;
            }
        }

        return false;
    }
}
