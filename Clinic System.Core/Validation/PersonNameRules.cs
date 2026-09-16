namespace Clinic_System.Core.Validation;

public static class PersonNameRules
{
    public const string InvalidNameMessage = "Name must not contain numbers or invalid characters";

    public static bool IsValid(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (name.Any(char.IsDigit))
            return false;

        var hasLetter = false;
        foreach (var c in name)
        {
            if (char.IsLetter(c))
            {
                hasLetter = true;
                continue;
            }

            if (c is ' ' or '-' or '\'' or '’' or '.')
                continue;

            return false;
        }

        return hasLetter;
    }

    public static string Sanitize(string? value, int maxLength, out bool removedInvalid)
    {
        var text = value ?? string.Empty;
        var cleaned = new string(text.Where(c =>
            char.IsLetter(c) || c is ' ' or '-' or '\'' or '’' or '.').ToArray());

        removedInvalid = cleaned != text;
        if (cleaned.Length > maxLength)
        {
            cleaned = cleaned[..maxLength];
            removedInvalid = true;
        }

        return cleaned;
    }

    public static string? Validate(string? name, bool required = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            return required ? "El nombre es obligatorio." : null;

        if (name.Length > PatientFieldLimits.FullName)
            return $"El nombre admite máximo {PatientFieldLimits.FullName} caracteres.";

        if (name.Any(char.IsDigit))
            return "El nombre no puede contener números.";

        if (!IsValid(name))
            return "El nombre solo puede incluir letras, espacios, guiones y apóstrofes.";

        return null;
    }
}
