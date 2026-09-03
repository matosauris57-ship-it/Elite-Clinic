using System.Globalization;
using System.Text.RegularExpressions;

namespace Clinic_System.Core.Odontogram;

public static partial class OdontogramSymbolColor
{
    [GeneratedRegex("^#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$", RegexOptions.CultureInvariant)]
    private static partial Regex HexRegex();

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && HexRegex().IsMatch(value.Trim());

    public static string Normalize(string? value, string fallback = "#757575")
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var hex = value.Trim();
        if (!IsValid(hex))
            return fallback;

        if (hex.Length == 4)
        {
            return $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}".ToUpperInvariant();
        }

        return hex.ToUpperInvariant();
    }

    public static string ToRgba(string? hex, double opacity)
    {
        var normalized = Normalize(hex);
        var alpha = Math.Clamp(opacity, 0.05, 1);
        if (!TryRgb(normalized, out var r, out var g, out var b))
            return $"rgba(117, 117, 117, {alpha.ToString("0.##", CultureInfo.InvariantCulture)})";

        return $"rgba({r}, {g}, {b}, {alpha.ToString("0.##", CultureInfo.InvariantCulture)})";
    }

    public static string ContrastInk(string? hex)
    {
        var normalized = Normalize(hex);
        if (!TryRgb(normalized, out var r, out var g, out var b))
            return "#FFFFFF";

        var luminance = (0.299 * r) + (0.587 * g) + (0.114 * b);
        return luminance > 160 ? "#1A1400" : "#FFFFFF";
    }

    public static bool TryRgb(string hex, out int r, out int g, out int b)
    {
        r = g = b = 0;
        var value = Normalize(hex);
        return value.Length == 7
            && int.TryParse(value[1..3], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r)
            && int.TryParse(value[3..5], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g)
            && int.TryParse(value[5..7], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b);
    }
}
