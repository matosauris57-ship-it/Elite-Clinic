using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class AttendanceConfirmationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _authenticatedApiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _apiSettings;

    public AttendanceConfirmationService(
        AuthenticatedApiClient authenticatedApiClient,
        IHttpClientFactory httpClientFactory,
        IOptions<ApiSettings> apiSettings)
    {
        _authenticatedApiClient = authenticatedApiClient;
        _httpClientFactory = httpClientFactory;
        _apiSettings = apiSettings.Value;
    }

    public async Task<(string? Url, AttendanceConfirmationTokenResult? Token, string? Error)> CreatePublicLinkAsync(
        int appointmentId,
        string currentBaseUri)
    {
        try
        {
            using var response = await _authenticatedApiClient.Client.PostAsync(
                $"/api/appointments/attendance-confirmation/{appointmentId}/token",
                null);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<AttendanceConfirmationTokenResult>>(JsonOptions);
            if (body?.Succeeded != true || body.Data == null)
                return (null, null, body?.Message ?? "No se pudo generar el enlace de confirmación.");

            var baseUrl = ResolvePublicBaseUrl(currentBaseUri);
            var url = $"{baseUrl}/confirmar-asistencia?token={Uri.EscapeDataString(body.Data.Token)}";
            return (url, body.Data, null);
        }
        catch (Exception ex)
        {
            return (null, null, ex.Message);
        }
    }

    public async Task<(AttendanceConfirmationDetails? Details, string? Error)> GetDetailsAsync(string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ClinicApiLogin");
            var url = $"/api/appointments/attendance-confirmation/details?token={Uri.EscapeDataString(token)}";
            var body = await client.GetFromJsonAsync<ApiResponse<AttendanceConfirmationDetails>>(url, JsonOptions);
            return body?.Succeeded == true
                ? (body.Data, null)
                : (null, body?.Message ?? "El enlace no es válido o expiró.");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Message, string? Error)> RespondAsync(string token, string action)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ClinicApiLogin");
            using var response = await client.PostAsJsonAsync(
                "/api/appointments/attendance-confirmation/respond",
                new { Token = token, Action = action },
                JsonOptions);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<AttendanceConfirmationResponseResult>>(JsonOptions);
            return body?.Succeeded == true
                ? (true, body.Message, null)
                : (false, null, body?.Message ?? "No se pudo registrar la respuesta.");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    private string ResolvePublicBaseUrl(string currentBaseUri)
    {
        var configured = _apiSettings.PublicBaseUrl?.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        return currentBaseUri.Trim().TrimEnd('/');
    }
}
