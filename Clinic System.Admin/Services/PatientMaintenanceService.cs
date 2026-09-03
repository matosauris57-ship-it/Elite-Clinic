using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class PatientMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public PatientMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<PatientListItem> Patients, string? Error)> GetPatientsAsync(bool includeInactive = true)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/patients?includeInactive={includeInactive.ToString().ToLowerInvariant()}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<PatientListItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return ([], body?.Message ?? "No se pudieron cargar los pacientes.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(PatientClinicalProfile? Profile, string? Error)> GetClinicalProfileAsync(int id)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/patients/{id}/clinical-profile");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PatientClinicalProfile>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return (null, body?.Message ?? "No se pudo cargar la ficha del paciente.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error, int? Id)> CreatePatientAsync(CreatePatientRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/patients", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CreatedPatientResponse>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (true, null, body.Data.Id);
            return (false, body?.Message ?? "No se pudo registrar el paciente.", null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message, null);
        }
    }

    public async Task<(bool Success, string? Error)> UpdatePatientAsync(int id, UpdatePatientRequest request)
    {
        try
        {
            request.Id = id;
            var response = await Client.PutAsJsonAsync($"/api/patients/{id}", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo actualizar el paciente.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DisablePatientAsync(int id)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/patients/{id}");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo deshabilitar el paciente.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> EnablePatientAsync(int id)
    {
        try
        {
            using var response = await Client.PutAsync($"/api/patients/{id}/enable", null);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo reactivar el paciente.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SaveDentalHistoryAsync(DentalHistoryForm form)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/dental/history", form, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudieron guardar los antecedentes.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<ToothRecordItem> Records, string? Error)> GetOdontogramAsync(int patientId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/dental/odontogram/patient/{patientId}");
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ToothRecordItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return ([], body?.Message ?? "No se pudo cargar el odontograma.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SaveOdontogramAsync(BatchOdontogramRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/dental/odontogram/batch", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo guardar el odontograma.");
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