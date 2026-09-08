using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Clinic_System.Application.DTOs.Patients;
using Clinic_System.Core.Enums;
using Clinic_System.Core.Validation;

namespace Clinic_System.Application.Common;

public static class PatientCsvImport
{
    public const int MaxRows = 1000;
    public const int MaxCsvBytes = 2 * 1024 * 1024;

    private static readonly Regex PhonePattern = new(@"^\+?[0-9]{10,15}$", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fullname"] = "FullName",
        ["nombre"] = "FullName",
        ["nombrecompleto"] = "FullName",
        ["fullnamecompleto"] = "FullName",
        ["fullname_name"] = "FullName",
        ["patientname"] = "FullName",
        ["gender"] = "Gender",
        ["genero"] = "Gender",
        ["género"] = "Gender",
        ["sexo"] = "Gender",
        ["dateofbirth"] = "DateOfBirth",
        ["fechanacimiento"] = "DateOfBirth",
        ["fecha_nacimiento"] = "DateOfBirth",
        ["nacimiento"] = "DateOfBirth",
        ["dob"] = "DateOfBirth",
        ["address"] = "Address",
        ["direccion"] = "Address",
        ["dirección"] = "Address",
        ["phone"] = "Phone",
        ["telefono"] = "Phone",
        ["teléfono"] = "Phone",
        ["phone_number"] = "Phone",
        ["nationalid"] = "NationalId",
        ["cedula"] = "NationalId",
        ["cédula"] = "NationalId",
        ["documento"] = "NationalId",
        ["email"] = "Email",
        ["correo"] = "Email",
        ["mail"] = "Email",
        ["mobilephone"] = "MobilePhone",
        ["celular"] = "MobilePhone",
        ["movil"] = "MobilePhone",
        ["móvil"] = "MobilePhone"
    };

    public static List<PatientImportConfirmRow> Parse(string csvContent, out string? parseError)
    {
        parseError = null;
        var rows = new List<PatientImportConfirmRow>();
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            parseError = "El archivo CSV está vacío.";
            return rows;
        }

        var text = csvContent.TrimStart('\uFEFF');
        var lines = SplitLines(text);
        if (lines.Count == 0)
        {
            parseError = "El archivo CSV está vacío.";
            return rows;
        }

        var delimiter = DetectDelimiter(lines[0]);
        var headerCells = ParseCsvLine(lines[0], delimiter);
        var map = BuildHeaderMap(headerCells);
        if (!map.ContainsKey("FullName") || !map.ContainsKey("Phone") || !map.ContainsKey("DateOfBirth")
            || !map.ContainsKey("Gender") || !map.ContainsKey("Address"))
        {
            parseError = "Faltan columnas obligatorias. Use la plantilla: FullName, Gender, DateOfBirth, Address, Phone.";
            return rows;
        }

        var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (dataLines.Count > MaxRows)
        {
            parseError = $"El archivo supera el máximo de {MaxRows} filas.";
            return rows;
        }

        for (var i = 0; i < dataLines.Count; i++)
        {
            var cells = ParseCsvLine(dataLines[i], delimiter);
            rows.Add(new PatientImportConfirmRow
            {
                RowNumber = i + 2, // + header
                FullName = Get(cells, map, "FullName"),
                Gender = Get(cells, map, "Gender"),
                DateOfBirth = ParseDate(Get(cells, map, "DateOfBirth")) ?? default,
                Address = Get(cells, map, "Address"),
                Phone = NormalizePhone(Get(cells, map, "Phone")),
                NationalId = NullIfEmpty(Get(cells, map, "NationalId")),
                Email = NullIfEmpty(Get(cells, map, "Email")),
                MobilePhone = NullIfEmpty(NormalizePhone(Get(cells, map, "MobilePhone")))
            });
        }

        return rows;
    }

    public static string? ValidateRow(
        PatientImportConfirmRow row,
        HashSet<string> usedPhones,
        HashSet<string> usedNationalIds,
        out Gender gender,
        out string? normalizedEmail)
    {
        gender = Gender.Male;
        normalizedEmail = null;

        if (string.IsNullOrWhiteSpace(row.FullName))
            return "El nombre es obligatorio.";
        if (row.FullName.Trim().Length > 100)
            return "El nombre no puede superar 100 caracteres.";

        if (string.IsNullOrWhiteSpace(row.Address))
            return "La dirección es obligatoria.";
        if (row.Address.Trim().Length > 200)
            return "La dirección no puede superar 200 caracteres.";

        if (string.IsNullOrWhiteSpace(row.Phone))
            return "El teléfono es obligatorio.";
        if (!PhonePattern.IsMatch(row.Phone))
            return "Teléfono inválido (10–15 dígitos, + opcional).";

        if (!TryParseGender(row.Gender, out gender))
            return "Género inválido. Use Male/Female o Masculino/Femenino.";

        if (row.DateOfBirth == default)
            return "Fecha de nacimiento inválida (use yyyy-MM-dd).";
        if (row.DateOfBirth.Date >= DateTime.Today)
            return "La fecha de nacimiento debe ser anterior a hoy.";

        if (!string.IsNullOrWhiteSpace(row.NationalId) && row.NationalId.Trim().Length > 20)
            return "La cédula no puede superar 20 caracteres.";

        if (!ContactEmail.TryValidate(row.Email, out normalizedEmail, out var emailError))
            return emailError;

        if (!string.IsNullOrWhiteSpace(row.MobilePhone) && !PhonePattern.IsMatch(row.MobilePhone))
            return "Celular inválido (10–15 dígitos, + opcional).";

        var phoneKey = row.Phone;
        if (!usedPhones.Add(phoneKey))
            return "Teléfono duplicado (ya existe o se repite en el archivo).";

        if (!string.IsNullOrWhiteSpace(row.NationalId))
        {
            var idKey = row.NationalId.Trim();
            if (!usedNationalIds.Add(idKey))
                return "Cédula duplicada (ya existe o se repite en el archivo).";
        }

        return null;
    }

    public static string BuildTemplateCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("FullName,Gender,DateOfBirth,Address,Phone,NationalId,Email,MobilePhone");
        sb.AppendLine("Ana Pérez,Female,1990-05-12,Calle Principal 123,+595981000001,1234567,ana@correo.com,+595981000002");
        sb.AppendLine("Juan López,Male,1985-11-03,Av. Central 45,+595981000003,7654321,,");
        return sb.ToString();
    }

    public static string NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var trimmed = value.Trim();
        var hasPlus = trimmed.StartsWith('+');
        var digits = Regex.Replace(trimmed, @"[^\d]", string.Empty);
        return hasPlus ? "+" + digits : digits;
    }

    private static bool TryParseGender(string? raw, out Gender gender)
    {
        gender = Gender.Male;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var value = raw.Trim();
        if (Enum.TryParse<Gender>(value, ignoreCase: true, out gender))
            return true;

        return value.ToLowerInvariant() switch
        {
            "m" or "masculino" or "hombre" or "male" => Assign(Gender.Male, out gender),
            "f" or "femenino" or "mujer" or "female" => Assign(Gender.Female, out gender),
            _ => false
        };

        static bool Assign(Gender g, out Gender result)
        {
            result = g;
            return true;
        }
    }

    private static DateTime? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var value = raw.Trim();
        string[] formats =
        [
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "d/M/yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy"
        ];

        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact.Date;

        if (DateTime.TryParse(value, CultureInfo.GetCultureInfo("es-DO"), DateTimeStyles.None, out var local))
            return local.Date;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var invariant))
            return invariant.Date;

        return null;
    }

    private static Dictionary<string, int> BuildHeaderMap(List<string> headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            var key = NormalizeHeader(headers[i]);
            if (string.IsNullOrEmpty(key))
                continue;
            if (HeaderAliases.TryGetValue(key, out var canonical))
                map[canonical] = i;
            else if (HeaderAliases.ContainsValue(key) || new[] { "FullName", "Gender", "DateOfBirth", "Address", "Phone", "NationalId", "Email", "MobilePhone" }
                         .Contains(key, StringComparer.OrdinalIgnoreCase))
                map[key] = i;
        }

        return map;
    }

    private static string NormalizeHeader(string header)
    {
        var cleaned = header.Trim().Trim('"').ToLowerInvariant();
        cleaned = cleaned.Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
        return cleaned;
    }

    private static string Get(List<string> cells, Dictionary<string, int> map, string key)
    {
        if (!map.TryGetValue(key, out var index) || index < 0 || index >= cells.Count)
            return string.Empty;
        return cells[index].Trim().Trim('"');
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static char DetectDelimiter(string headerLine)
    {
        var commas = headerLine.Count(c => c == ',');
        var semis = headerLine.Count(c => c == ';');
        return semis > commas ? ';' : ',';
    }

    private static List<string> SplitLines(string text) =>
        text.Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.None)
            .Select(l => l.TrimEnd())
            .ToList();

    private static List<string> ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (ch == delimiter && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        result.Add(current.ToString());
        return result;
    }
}
