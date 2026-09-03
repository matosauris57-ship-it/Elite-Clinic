using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class MedicalConditionMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public MedicalConditionMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<MedicalConditionListItem> Items, string? Error)> GetConditionsAsync(bool activeOnly = false)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/medical-conditions?activeOnly={activeOnly.ToString().ToLowerInvariant()}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<MedicalConditionListItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return ([], body?.Message ?? "No se pudieron cargar las enfermedades.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error, int? Id)> CreateAsync(CreateMedicalConditionRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/medical-conditions", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<MedicalConditionListItem>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (true, null, body.Data.Id);
            return (false, body?.Message ?? "No se pudo crear la enfermedad.", null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message, null);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateMedicalConditionRequest request)
    {
        try
        {
            request.Id = id;
            var response = await Client.PutAsJsonAsync($"/api/medical-conditions/{id}", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<MedicalConditionListItem>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo actualizar la enfermedad.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/medical-conditions/{id}");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo eliminar la enfermedad.");
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
