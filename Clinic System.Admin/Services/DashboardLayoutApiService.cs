using System.Net.Http.Json;
using System.Text.Json;
using Clinic_System.Core.Dashboard;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class DashboardLayoutApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public DashboardLayoutApiService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public Task<(DashboardLayoutResponse? Data, string? Error)> GetLayoutAsync() =>
        GetAsync<DashboardLayoutResponse>("/api/dashboard/layout", "No se pudo cargar el Dashboard.");

    public Task<(DashboardLayoutResponse? Data, string? Error)> SaveLayoutAsync(DashboardLayoutDocument layout) =>
        SendAsync<DashboardLayoutResponse>(HttpMethod.Put, "/api/dashboard/layout", new { layout }, "No se pudo guardar el Dashboard.");

    public Task<(DashboardLayoutResponse? Data, string? Error)> RestoreLayoutAsync() =>
        SendAsync<DashboardLayoutResponse>(HttpMethod.Post, "/api/dashboard/layout/restore", null, "No se pudo restaurar el Dashboard.");

    public Task<(DashboardClinicConfigResponse? Data, string? Error)> GetClinicConfigAsync() =>
        GetAsync<DashboardClinicConfigResponse>("/api/dashboard/clinic", "No se pudo cargar la configuración de widgets.");

    public Task<(DashboardClinicConfigResponse? Data, string? Error)> SaveClinicConfigAsync(DashboardLayoutDocument layout) =>
        SendAsync<DashboardClinicConfigResponse>(HttpMethod.Put, "/api/dashboard/clinic", new { layout }, "No se pudo guardar la configuración.");

    public Task<(DashboardClinicConfigResponse? Data, string? Error)> RestoreClinicConfigAsync() =>
        SendAsync<DashboardClinicConfigResponse>(HttpMethod.Post, "/api/dashboard/clinic/restore", null, "No se pudo restaurar la configuración.");

    public Task<(PatientDashboardStats? Data, string? Error)> GetPatientStatsAsync() =>
        GetAsync<PatientDashboardStats>("/api/dashboard/patient-stats", "No se pudieron cargar las estadísticas de pacientes.");

    public Task<(List<RecentClinicalActivityItem>? Data, string? Error)> GetRecentActivityAsync(string period, int take) =>
        GetAsync<List<RecentClinicalActivityItem>>(
            $"/api/dashboard/recent-activity?period={Uri.EscapeDataString(period)}&take={take}",
            "No se pudo cargar la actividad reciente.");

    public Task<(PeriodontalIncompleteStats? Data, string? Error)> GetPeriodontalIncompleteAsync() =>
        GetAsync<PeriodontalIncompleteStats>("/api/dashboard/periodontal-incomplete", "No se pudieron cargar los periodontogramas.");

    private async Task<(T? Data, string? Error)> GetAsync<T>(string url, string fallback)
    {
        try
        {
            using var response = await Client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (default, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (default, "No autorizado.");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (default, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return (default, body?.Message ?? fallback);
        }
        catch (Exception ex)
        {
            return (default, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private async Task<(T? Data, string? Error)> SendAsync<T>(HttpMethod method, string url, object? body, string fallback)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (body != null)
                request.Content = JsonContent.Create(body, options: JsonOptions);
            using var response = await Client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (default, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (default, "No autorizado.");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (default, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
            if (payload?.Succeeded == true && payload.Data != null)
                return (payload.Data, null);
            return (default, payload?.Message ?? fallback);
        }
        catch (Exception ex)
        {
            return (default, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;
}
