using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class DoctorMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public DoctorMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<DoctorListItem> Doctors, string? Error)> GetDoctorsAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/doctors?includeInactive=true");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], GetUnauthorizedMessage());

            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<DoctorListItem>>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return ([], body?.Message ?? "No se pudieron cargar los médicos.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(DoctorListItem? Doctor, string? Error)> GetDoctorByIdAsync(int id)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/doctors/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, GetUnauthorizedMessage());

            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<DoctorListItem>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (null, body?.Message ?? "No se pudo cargar el médico.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error, int? Id)> CreateDoctorAsync(CreateDoctorRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/doctors", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CreatedDoctorResponse>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (true, null, body.Data.Id);

            return (false, FormatApiErrors(body), null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message, null);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateDoctorAsync(int id, UpdateDoctorRequest request)
    {
        try
        {
            request.Id = id;
            var response = await Client.PutAsJsonAsync($"/api/doctors/{id}", request, JsonOptions);
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

    public async Task<(bool Success, string? Error)> DisableDoctorAsync(int id)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/doctors/{id}");
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

    public async Task<(bool Success, string? Error)> EnableDoctorAsync(int id)
    {
        try
        {
            using var response = await Client.PutAsync($"/api/doctors/{id}/enable", null);
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

    private static string FormatApiErrors<T>(ApiResponse<T>? body)
    {
        if (body?.Errors?.Count > 0)
            return string.Join(" · ", body.Errors.Select(TranslateError));

        return body?.Message ?? "No se pudo completar la operación.";
    }

    private static string TranslateError(string error) => error switch
    {
        var e when e.Contains("Doctor Name is required", StringComparison.OrdinalIgnoreCase) =>
            "El nombre del médico es obligatorio.",
        var e when e.Contains("Name must not exceed", StringComparison.OrdinalIgnoreCase) =>
            "El nombre no puede superar 100 caracteres.",
        var e when e.Contains("Address is required", StringComparison.OrdinalIgnoreCase) =>
            "La dirección es obligatoria.",
        var e when e.Contains("Specialization is required", StringComparison.OrdinalIgnoreCase) =>
            "La especialización es obligatoria.",
        var e when e.Contains("Username must start", StringComparison.OrdinalIgnoreCase) =>
            "El usuario debe empezar con letra e incluir al menos un número.",
        var e when e.Contains("Email is already", StringComparison.OrdinalIgnoreCase) =>
            "Este correo ya está registrado.",
        var e when e.Contains("Username is already", StringComparison.OrdinalIgnoreCase) =>
            "Este nombre de usuario ya existe.",
        var e when e.Contains("Phone number is already", StringComparison.OrdinalIgnoreCase) =>
            "Este teléfono ya está registrado.",
        var e when e.Contains("Phone number must contain", StringComparison.OrdinalIgnoreCase) ||
                   e.Contains("Phone number is required", StringComparison.OrdinalIgnoreCase) =>
            "El teléfono debe tener 10–15 dígitos (solo números).",
        var e when e.Contains("Invalid email", StringComparison.OrdinalIgnoreCase) =>
            "Correo electrónico inválido.",
        var e when e.Contains("Date of Birth", StringComparison.OrdinalIgnoreCase) =>
            "La fecha de nacimiento no es válida.",
        var e when e.Contains("Password must", StringComparison.OrdinalIgnoreCase) ||
                   e.Contains("Password is required", StringComparison.OrdinalIgnoreCase) =>
            "La contraseña debe tener al menos 6 caracteres.",
        var e when e.Contains("Password and Confirm Password", StringComparison.OrdinalIgnoreCase) =>
            "Las contraseñas no coinciden.",
        _ => error.Contains(':') ? error[(error.IndexOf(':') + 1)..].Trim() : error
    };
}
