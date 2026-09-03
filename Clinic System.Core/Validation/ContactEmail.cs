using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Clinic_System.Core.Validation;

public static class ContactEmail
{
    public const int MaxLength = 120;

    private static readonly Regex LocalPart = new(
        @"^[A-Za-z0-9](?:[A-Za-z0-9._%+\-]*[A-Za-z0-9])?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> ReservedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "example.com",
        "example.net",
        "example.org",
        "invalid",
        "test"
    };

    private static readonly HashSet<string> ReservedTlds = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "local",
        "invalid",
        "test",
        "example"
    };

    public static bool IsOmitted(string? value) => string.IsNullOrWhiteSpace(value);

    public static string? NormalizeOrNull(string? value) =>
        IsOmitted(value) ? null : value!.Trim().ToLowerInvariant();

    public static bool TryValidate(string? value, out string? normalized, out string? error)
    {
        normalized = null;
        error = null;

        if (IsOmitted(value))
            return true;

        var trimmed = value!.Trim();
        if (trimmed.Length > MaxLength)
        {
            error = "El correo no puede superar 120 caracteres.";
            return false;
        }

        if (trimmed.Contains(' ', StringComparison.Ordinal) || trimmed.Contains("..", StringComparison.Ordinal))
        {
            error = "El correo no tiene una sintaxis válida.";
            return false;
        }

        MailAddress address;
        try
        {
            address = new MailAddress(trimmed);
        }
        catch (FormatException)
        {
            error = "El correo no tiene una sintaxis válida.";
            return false;
        }

        if (!string.Equals(address.Address, trimmed, StringComparison.OrdinalIgnoreCase))
        {
            error = "El correo no tiene una sintaxis válida.";
            return false;
        }

        var at = address.Address.LastIndexOf('@');
        if (at <= 0 || at != address.Address.IndexOf('@'))
        {
            error = "El correo no tiene una sintaxis válida.";
            return false;
        }

        var local = address.Address[..at];
        var host = address.Host;
        if (!LocalPart.IsMatch(local))
        {
            error = "El correo no tiene una sintaxis válida.";
            return false;
        }

        if (Uri.CheckHostName(host) != UriHostNameType.Dns)
        {
            error = "El dominio del correo no es válido.";
            return false;
        }

        var dot = host.LastIndexOf('.');
        if (dot <= 0 || dot == host.Length - 1)
        {
            error = "El dominio del correo debe incluir una extensión (por ejemplo .com).";
            return false;
        }

        var tld = host[(dot + 1)..];
        if (tld.Length < 2 || !tld.All(char.IsLetter))
        {
            error = "La extensión del dominio no es válida.";
            return false;
        }

        if (ReservedHosts.Contains(host) || ReservedTlds.Contains(tld))
        {
            error = "El dominio del correo no es un dominio real utilizable.";
            return false;
        }

        var labels = host.Split('.');
        if (labels.Any(label => label.Length == 0 || label.StartsWith('-') || label.EndsWith('-')))
        {
            error = "El dominio del correo no es válido.";
            return false;
        }

        normalized = address.Address.ToLowerInvariant();
        return true;
    }
}
