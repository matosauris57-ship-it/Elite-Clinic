using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class CampaignBookingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public CampaignBookingService(
        IHttpClientFactory httpClientFactory,
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient AnonymousClient => _httpClientFactory.CreateClient("ClinicApiLogin");
    private HttpClient AuthClient => _apiClient.Client;

    public async Task<(CampaignBookingContext? Data, string? Error)> GetContextAsync(string token)
    {
        try
        {
            var url = $"/api/campaign-booking/context?token={Uri.EscapeDataString(token)}";
            var body = await AnonymousClient.GetFromJsonAsync<ApiResponse<CampaignBookingContext>>(url, JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : (null, body?.Message ?? "El enlace no es válido o expiró.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<string> Occupied, string? Error)> GetOccupiedAsync(string token, int doctorId, DateTime date)
    {
        try
        {
            var url = $"/api/campaign-booking/occupied?token={Uri.EscapeDataString(token)}&doctorId={doctorId}&date={date:yyyy-MM-dd}";
            var body = await AnonymousClient.GetFromJsonAsync<ApiResponse<List<string>>>(url, JsonOptions);
            return body?.Succeeded == true
                ? (body.Data ?? [], null)
                : ([], body?.Message ?? "No se pudieron cargar los horarios ocupados.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(CampaignBookingResult? Data, string? Error)> RequestAsync(CampaignBookFormRequest request)
    {
        try
        {
            using var response = await AnonymousClient.PostAsJsonAsync("/api/campaign-booking/request", request, JsonOptions);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignBookingResult>>(JsonOptions);
            if (body?.Succeeded == true)
                return (body.Data, null);

            if (body?.Data != null && response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (body.Data, body.Message ?? "Ya envió una solicitud con este enlace.");

            return (null, body?.Message ?? "No se pudo enviar la solicitud.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<CampaignAppointmentRequestItem> Items, string? Error)> ListRequestsAsync(string? status = null, int take = 50)
    {
        try
        {
            var url = $"/api/campaign-booking/requests?take={take}";
            if (!string.IsNullOrWhiteSpace(status))
                url += $"&status={Uri.EscapeDataString(status)}";

            using var response = await AuthClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return ([], "No autorizado.");

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<CampaignAppointmentRequestItem>>>(JsonOptions);
            return body?.Succeeded == true
                ? (body.Data ?? [], null)
                : ([], body?.Message ?? "No se pudieron cargar las solicitudes.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(CampaignAppointmentRequestItem? Data, string? Error)> ScheduleAsync(int id, CampaignScheduleFormRequest request)
    {
        try
        {
            using var response = await AuthClient.PostAsJsonAsync($"/api/campaign-booking/requests/{id}/schedule", request, JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (null, "No autorizado.");

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignAppointmentRequestItem>>(JsonOptions);
            return body?.Succeeded == true
                ? (body.Data, body.Message)
                : (null, body?.Message ?? "No se pudo agendar la cita.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(CampaignAppointmentRequestItem? Data, string? Error)> RejectAsync(int id, string? staffNote)
    {
        try
        {
            using var response = await AuthClient.PostAsJsonAsync(
                $"/api/campaign-booking/requests/{id}/reject",
                new { staffNote },
                JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (null, "No autorizado.");

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignAppointmentRequestItem>>(JsonOptions);
            return body?.Succeeded == true
                ? (body.Data, body.Message)
                : (null, body?.Message ?? "No se pudo rechazar la solicitud.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(CampaignBookingNotifySettingsModel? Data, string? Error)> GetNotifySettingsAsync()
    {
        try
        {
            using var response = await AuthClient.GetAsync("/api/clinic/campaign-booking-notify");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (null, "No autorizado.");

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignBookingNotifySettingsModel>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : (null, body?.Message ?? "No se pudo cargar la configuración.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Ok, string? Error)> SaveNotifySettingsAsync(CampaignBookingNotifySettingsModel settings)
    {
        try
        {
            using var response = await AuthClient.PutAsJsonAsync("/api/clinic/campaign-booking-notify", settings, JsonOptions);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (false, "No autorizado.");

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignBookingNotifySettingsModel>>(JsonOptions);
            return body?.Succeeded == true
                ? (true, null)
                : (false, body?.Message ?? "No se pudo guardar.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;
}
