using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class TreatmentProcedureMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public TreatmentProcedureMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public Task<(List<TreatmentProcedureListItem> Items, string? Error)> GetProceduresAsync(bool activeOnly = false, int? doctorId = null) =>
        GetProceduresInternalAsync(activeOnly, doctorId);

    public Task<(List<TreatmentProcedureListItem> Items, string? Error)> GetActiveProceduresAsync(int? doctorId = null) =>
        GetProceduresInternalAsync(activeOnly: true, doctorId);

    private async Task<(List<TreatmentProcedureListItem> Items, string? Error)> GetProceduresInternalAsync(bool activeOnly, int? doctorId = null)
    {
        try
        {
            var url = $"/api/treatment-procedures?activeOnly={activeOnly.ToString().ToLowerInvariant()}";
            if (doctorId.HasValue)
                url += $"&doctorId={doctorId.Value}";

            using var response = await Client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], GetUnauthorizedMessage());

            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<TreatmentProcedureListItem>>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return ([], body?.Message ?? "No se pudieron cargar los procedimientos.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error, int? Id)> CreateProcedureAsync(CreateTreatmentProcedureRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/treatment-procedures", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<TreatmentProcedureListItem>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (true, null, body.Data.Id);

            return (false, FormatApiErrors(body), null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message, null);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateProcedureAsync(int id, UpdateTreatmentProcedureRequest request)
    {
        try
        {
            request.Id = id;
            var response = await Client.PutAsJsonAsync($"/api/treatment-procedures/{id}", request, JsonOptions);
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

    public async Task<(bool Success, string? Error)> DeleteProcedureAsync(int id)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/treatment-procedures/{id}");
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

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private static string FormatApiErrors<T>(ApiResponse<T>? body) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message ?? "No se pudo completar la operación.";
}
