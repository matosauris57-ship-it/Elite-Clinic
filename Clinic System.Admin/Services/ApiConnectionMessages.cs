using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;

namespace DentalCare.Admin.Services;

public static class ApiConnectionMessages
{
    public const string RateLimitedMessage = "Demasiadas solicitudes. Espera un momento e inténtalo de nuevo.";

    public static bool IsConnectionFailure(Exception ex) =>
        ex is HttpRequestException or SocketException ||
        (ex.InnerException != null && IsConnectionFailure(ex.InnerException));

    public static bool IsRateLimited(HttpResponseMessage response) =>
        response.StatusCode == HttpStatusCode.TooManyRequests;

    public static async Task<string> GetRateLimitMessageAsync(HttpResponseMessage response)
    {
        if (!IsRateLimited(response))
            return RateLimitedMessage;

        try
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return RateLimitedMessage;

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("message", out var messageElement))
            {
                var text = messageElement.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }
        catch (JsonException)
        {
            // ignore malformed body
        }

        return RateLimitedMessage;
    }

    public static string ApiUnavailable(string apiBaseUrl) =>
        $"No se pudo conectar con la API ({apiBaseUrl}). Inicia el proyecto Elite Clinic y vuelve a intentar.";

    public static string UnauthorizedSession(TokenStorage tokenStorage) =>
        string.IsNullOrWhiteSpace(tokenStorage.RefreshToken)
            ? "Sesión incompleta. Cierra sesión e inicia de nuevo."
            : string.IsNullOrWhiteSpace(tokenStorage.AccessToken)
                ? "No se pudo cargar el token de sesión. Cierra sesión e inicia de nuevo."
                : "No se pudo renovar la sesión. Cierra sesión e inicia de nuevo.";

    public static bool IsSessionError(string? error) =>
        !string.IsNullOrWhiteSpace(error) &&
        (error.Contains("sesión", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("sesion", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("token", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("Cierra sesión", StringComparison.OrdinalIgnoreCase));
}
