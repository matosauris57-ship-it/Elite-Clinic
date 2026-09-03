using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class ClinicalTreatmentMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public ClinicalTreatmentMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<DentalTreatmentListItem> Items, string? Error)> GetByPatientAsync(int patientId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/dental/treatments/patient/{patientId}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], GetUnauthorizedMessage());
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<DentalTreatmentListItem>>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : ([], FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> CreateTreatmentAsync(CreateDentalTreatmentRequest request)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync("/api/dental/treatments", request, JsonOptions);
            return await ReadOperationAsync(response);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<DentalTreatmentListItem> Items, string? Error)> GetTreatmentsAsync(string? search, string? status)
    {
        var (page, error) = await GetTreatmentsPageAsync(search, status);
        return (page.Items, error);
    }

    public async Task<(DentalTreatmentsAdminPage Page, string? Error)> GetTreatmentsPageAsync(
        string? search = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? pageNumber = null,
        int? pageSize = null)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"Search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(status))
                query.Add($"Status={Uri.EscapeDataString(status)}");
            if (fromDate.HasValue)
                query.Add($"FromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue)
                query.Add($"ToDate={toDate.Value:yyyy-MM-dd}");
            if (pageNumber.HasValue)
                query.Add($"PageNumber={pageNumber.Value}");
            if (pageSize.HasValue)
                query.Add($"PageSize={pageSize.Value}");

            var url = query.Count > 0
                ? $"/api/dental/treatments?{string.Join("&", query)}"
                : "/api/dental/treatments";

            using var response = await Client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (new DentalTreatmentsAdminPage(), GetUnauthorizedMessage());

            if (ApiConnectionMessages.IsRateLimited(response))
                return (new DentalTreatmentsAdminPage(), await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<DentalTreatmentsAdminPage>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
            {
                body.Data.Items ??= [];
                return (body.Data, null);
            }

            return (new DentalTreatmentsAdminPage(), body?.Message ?? "No se pudieron cargar los tratamientos clínicos.");
        }
        catch (Exception ex)
        {
            return (new DentalTreatmentsAdminPage(), FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> UpdateTreatmentAsync(int id, UpdateDentalTreatmentRequest request)
    {
        try
        {
            request.Id = id;
            var response = await Client.PutAsJsonAsync($"/api/dental/treatments/{id}", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> StartTreatmentAsync(int id)
    {
        try
        {
            using var response = await Client.PutAsync($"/api/dental/treatments/{id}/start", null);
            return await ReadOperationAsync(response);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> CompleteTreatmentAsync(
        int id, CompleteDentalTreatmentRequest? request = null)
    {
        try
        {
            using var response = request == null
                ? await Client.PutAsync($"/api/dental/treatments/{id}/complete", null)
                : await Client.PutAsJsonAsync($"/api/dental/treatments/{id}/complete", request, JsonOptions);
            return await ReadOperationAsync(response);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> CancelTreatmentAsync(int id, string? reason)
    {
        try
        {
            var response = await Client.PutAsJsonAsync(
                $"/api/dental/treatments/{id}/cancel",
                new CancelDentalTreatmentRequest { Reason = reason },
                JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteTreatmentAsync(int id)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/dental/treatments/{id}");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private string GetUnauthorizedMessage() =>
        ApiConnectionMessages.UnauthorizedSession(_tokenStorage);

    private static async Task<(bool Success, string? Error)> ReadOperationAsync(HttpResponseMessage response)
    {
        if (ApiConnectionMessages.IsRateLimited(response))
            return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        return body?.Succeeded == true ? (true, null) : (false, FormatApiErrors(body));
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private static string FormatApiErrors<T>(ApiResponse<T>? body) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message ?? "No se pudo completar la operación.";
}
