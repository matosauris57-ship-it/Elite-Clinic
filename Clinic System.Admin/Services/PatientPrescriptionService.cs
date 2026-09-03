using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class PatientPrescriptionService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public PatientPrescriptionService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public Task<(List<PrescriptionTemplate> Items, string? Error)> ListTemplatesAsync() =>
        GetListAsync<PrescriptionTemplate>("/api/dental/prescriptions/templates", "No se pudieron cargar las plantillas de receta.");

    public Task<(List<PatientPrescriptionSummary> Items, string? Error)> ListAsync(int patientId) =>
        GetListAsync<PatientPrescriptionSummary>(
            $"/api/dental/prescriptions/patient/{patientId}",
            "No se pudieron cargar las recetas del paciente.");

    public Task<(PatientPrescriptionDetail? Item, string? Error)> GetAsync(int prescriptionId) =>
        GetSingleAsync<PatientPrescriptionDetail>(
            $"/api/dental/prescriptions/{prescriptionId}",
            "No se pudo cargar la receta.");

    public Task<(PatientPrescriptionDetail? Item, string? Error)> CreateAsync(int patientId, SavePatientPrescriptionRequest request) =>
        SendAsync<PatientPrescriptionDetail>(
            HttpMethod.Post,
            $"/api/dental/prescriptions/patient/{patientId}",
            request,
            "No se pudo guardar la receta.");

    public Task<(PatientPrescriptionDetail? Item, string? Error)> UpdateAsync(int prescriptionId, SavePatientPrescriptionRequest request) =>
        SendAsync<PatientPrescriptionDetail>(
            HttpMethod.Put,
            $"/api/dental/prescriptions/{prescriptionId}",
            request,
            "No se pudo actualizar la receta.");

    public Task<(bool Success, string? Error)> DeleteAsync(int prescriptionId) =>
        SendStatusAsync(HttpMethod.Delete, $"/api/dental/prescriptions/{prescriptionId}", null, "No se pudo anular la receta.");

    private async Task<(List<T> Items, string? Error)> GetListAsync<T>(string url, string fallback)
    {
        try
        {
            using var response = await Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<T>>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : ([], FormatApiError(body, fallback));
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private async Task<(T? Item, string? Error)> GetSingleAsync<T>(string url, string fallback) where T : class
    {
        try
        {
            using var response = await Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : (null, FormatApiError(body, fallback));
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private async Task<(T? Item, string? Error)> SendAsync<T>(HttpMethod method, string url, object? payload, string fallback) where T : class
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (payload != null)
                request.Content = JsonContent.Create(payload, options: JsonOptions);
            using var response = await Client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : (null, FormatApiError(body, fallback));
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private async Task<(bool Success, string? Error)> SendStatusAsync(HttpMethod method, string url, object? payload, string fallback)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (payload != null)
                request.Content = JsonContent.Create(payload, options: JsonOptions);
            using var response = await Client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (false, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(JsonOptions);
            return body?.Succeeded == true
                ? (true, null)
                : (false, FormatApiError(body, fallback));
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

    private static string FormatApiError<T>(ApiResponse<T>? body, string fallback) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message ?? fallback;
}
