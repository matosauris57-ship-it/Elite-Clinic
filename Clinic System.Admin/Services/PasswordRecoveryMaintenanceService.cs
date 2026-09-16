using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;

namespace DentalCare.Admin.Services;

public class PasswordRecoveryMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;

    public PasswordRecoveryMaintenanceService(AuthenticatedApiClient apiClient, TokenStorage tokenStorage)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(PasswordRecoveryListResponse List, string? Error)> GetListAsync(string? status)
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(status)
                ? "/api/password-recovery"
                : $"/api/password-recovery?status={Uri.EscapeDataString(status)}";
            using var response = await Client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (new PasswordRecoveryListResponse(), ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (new PasswordRecoveryListResponse(), "No tiene permiso para ver estas solicitudes.");

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PasswordRecoveryListResponse>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (new PasswordRecoveryListResponse(), body?.Message ?? "No se pudieron cargar las solicitudes.");
        }
        catch (Exception ex)
        {
            return (new PasswordRecoveryListResponse(), FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(int Count, string? Error)> GetPendingCountAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/password-recovery/pending-count");
            if (!response.IsSuccessStatusCode)
                return (0, null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<int>>(JsonOptions);
            return body?.Succeeded == true ? (body.Data, null) : (0, null);
        }
        catch
        {
            return (0, null);
        }
    }

    public async Task<(bool Success, string? Error)> ResolveAsync(int id, bool dismiss, string? note)
    {
        try
        {
            var path = dismiss ? $"/api/password-recovery/{id}/dismiss" : $"/api/password-recovery/{id}/resolve";
            using var response = await Client.PutAsJsonAsync(path, new { Note = note }, JsonOptions);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PasswordRecoveryRequestItem>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);

            return (false, body?.Message ?? "No se pudo actualizar la solicitud.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private static string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex) ? "No se pudo conectar con la API." : null;
}
