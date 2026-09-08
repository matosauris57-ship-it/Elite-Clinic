using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class ReportMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public ReportMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(ClinicReportsData? Data, string? Error)> GetReportsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var from = fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var to = toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var url = $"/api/reports?fromDate={from}&toDate={to}";
            using var response = await Client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (null, "No tiene permiso para ver reportes.");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<ClinicReportsData>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (null, FormatApiErrors(body) ?? "No se pudieron cargar los reportes.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private static string? FormatApiErrors(ApiResponse<ClinicReportsData>? body)
    {
        if (body == null)
            return null;
        if (!string.IsNullOrWhiteSpace(body.Message))
            return body.Message;
        return null;
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;
}
