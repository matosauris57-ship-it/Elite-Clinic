using System.Globalization;
using System.Text;

namespace Clinic_System.Core.Finance;

public static class Money
{
    public const int Scale = 2;
    public static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("es-DO");

    public static decimal Normalize(decimal value) =>
        decimal.Round(value, Scale, MidpointRounding.AwayFromZero);

    public static decimal Multiply(decimal unitPrice, int quantity) =>
        Normalize(Normalize(unitPrice) * quantity);

    public static decimal Sum(IEnumerable<decimal> values) =>
        Normalize(values.Aggregate(0m, (total, item) => total + Normalize(item)));

    public static decimal MaxZero(decimal value)
    {
        var rounded = Normalize(value);
        return rounded < 0 ? 0 : rounded;
    }

    public static string Format(decimal value) =>
        Normalize(value).ToString("C", DisplayCulture);

    public static string FormatRange(decimal min, decimal max)
    {
        var from = Normalize(min);
        var to = Normalize(max);
        return from == to ? Format(from) : $"{Format(from)} – {Format(to)}";
    }

    public static string ToInput(decimal value) =>
        Normalize(value).ToString("0.00", CultureInfo.InvariantCulture);

    public static decimal? Resolve(string? input, decimal? fallback)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (!TryParse(input, out var parsed) || parsed <= 0)
                throw new InvalidOperationException("El monto no es válido. Use un número con hasta dos decimales.");
            return parsed;
        }

        return fallback.HasValue ? Normalize(fallback.Value) : null;
    }

    public static bool TryParse(string? input, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var text = StripCurrency(input);
        if (text.Length == 0)
            return false;

        var lastComma = text.LastIndexOf(',');
        var lastDot = text.LastIndexOf('.');
        if (lastComma >= 0 && lastDot >= 0)
        {
            var decimalSep = lastComma > lastDot ? ',' : '.';
            var thousandSep = decimalSep == ',' ? '.' : ',';
            text = text.Replace(thousandSep.ToString(CultureInfo.InvariantCulture), string.Empty);
            if (decimalSep == ',')
                text = text.Replace(',', '.');
        }
        else if (lastComma >= 0)
        {
            var decimals = text.Length - lastComma - 1;
            text = decimals is 1 or 2
                ? text.Replace(',', '.')
                : text.Replace(",", string.Empty);
        }
        else if (lastDot >= 0)
        {
            var decimals = text.Length - lastDot - 1;
            if (decimals is not (1 or 2))
                text = text.Replace(".", string.Empty);
        }

        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) || parsed < 0)
            return false;

        amount = Normalize(parsed);
        return true;
    }

    private static string StripCurrency(string input)
    {
        var builder = new StringBuilder(input.Length);
        foreach (var ch in input.Trim())
        {
            if (char.IsDigit(ch) || ch is '.' or ',' or '-')
                builder.Append(ch);
        }

        return builder.ToString();
    }
}
