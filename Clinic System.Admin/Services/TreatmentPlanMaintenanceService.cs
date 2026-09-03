using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class TreatmentPlanMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthenticatedApiClient apiClient;
    private readonly TokenStorage tokenStorage;
    private readonly ApiSettings apiSettings;

    public TreatmentPlanMaintenanceService(
        AuthenticatedApiClient apiClient, TokenStorage tokenStorage, IOptions<ApiSettings> apiSettings)
    {
        this.apiClient = apiClient;
        this.tokenStorage = tokenStorage;
        this.apiSettings = apiSettings.Value;
    }

    public async Task<(List<TreatmentPlanListItem> Items, string? Error)> GetByPatientAsync(int patientId)
    {
        try
        {
            using var response = await apiClient.Client.GetAsync($"/api/dental/treatment-plans/patient/{patientId}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<TreatmentPlanListItem>>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : ([], FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return ([], ConnectionError(ex));
        }
    }

    public async Task<(bool Success, string? Error)> CreateAsync(CreateTreatmentPlanRequest request)
    {
        try
        {
            using var response = await apiClient.Client.PostAsJsonAsync("/api/dental/treatment-plans", request, JsonOptions);
            return await ReadOperationAsync(response);
        }
        catch (Exception ex)
        {
            return (false, ConnectionError(ex));
        }
    }

    public Task<(bool Success, string? Error)> ApproveAsync(int id) => PutAsync(id, "approve", null);
    public Task<(bool Success, string? Error)> CompleteAsync(int id) => PutAsync(id, "complete", null);
    public Task<(bool Success, string? Error)> RejectAsync(int id, string? reason) =>
        PutAsync(id, "reject", new RejectTreatmentPlanRequest { Reason = reason });

    private async Task<(bool Success, string? Error)> PutAsync(int id, string action, object? payload)
    {
        try
        {
            using var response = payload == null
                ? await apiClient.Client.PutAsync($"/api/dental/treatment-plans/{id}/{action}", null)
                : await apiClient.Client.PutAsJsonAsync($"/api/dental/treatment-plans/{id}/{action}", payload, JsonOptions);
            return await ReadOperationAsync(response);
        }
        catch (Exception ex)
        {
            return (false, ConnectionError(ex));
        }
    }

    private static async Task<(bool Success, string? Error)> ReadOperationAsync(HttpResponseMessage response)
    {
        if (ApiConnectionMessages.IsRateLimited(response))
            return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        return body?.Succeeded == true ? (true, null) : (false, FormatApiErrors(body));
    }

    private string ConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(apiSettings.ApiBaseUrl)
            : ex.Message;

    private static string FormatApiErrors<T>(ApiResponse<T>? body) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message ?? "No se pudo completar la operación.";
}
