using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class AccessControlMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SessionExpiredMessage = "Sesión expirada. Cierra sesión e inicia de nuevo.";
    private const string ForbiddenMessage = "No tienes permisos para esta acción.";
    private const string InvalidResponseMessage = "Respuesta inválida del servidor. Verifica que la API esté actualizada.";

    private readonly AuthenticatedApiClient _apiClient;
    private readonly ApiSettings _apiSettings;

    public AccessControlMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(PermissionCatalogResponse? Catalog, string? Error)> GetCatalogAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/access/permissions/catalog");
            var (data, error) = await ParseApiResponseAsync<PermissionCatalogResponse>(response);
            if (error != null)
                return (null, error);

            return data != null ? (data, null) : (null, "No se pudo cargar el catálogo de permisos.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<RoleListItem> Roles, string? Error)> GetRolesAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/access/roles");
            var (data, error) = await ParseApiResponseAsync<List<RoleListItem>>(response);
            if (error != null)
                return ([], error);

            return data != null ? (data, null) : ([], "No se pudieron cargar los roles.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(RolePermissionsResponse? Role, string? Error)> GetRolePermissionsAsync(string roleId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/access/roles/{roleId}/permissions");
            var (data, error) = await ParseApiResponseAsync<RolePermissionsResponse>(response);
            if (error != null)
                return (null, error);

            return data != null ? (data, null) : (null, "No se pudieron cargar los permisos del rol.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error, RoleListItem? Role)> CreateRoleAsync(string name)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync("/api/access/roles", new CreateRoleRequest { Name = name });
            var (data, error) = await ParseApiResponseAsync<RoleListItem>(response);
            if (error != null)
                return (false, error, null);

            return data != null ? (true, null, data) : (false, "No se pudo crear el rol.", null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message, null);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateRolePermissionsAsync(string roleId, List<string> permissions)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync(
                $"/api/access/roles/{roleId}/permissions",
                new UpdateRolePermissionsRequest { Permissions = permissions });

            var (_, error) = await ParseApiResponseAsync<string>(response);
            return error == null ? (true, null) : (false, error);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteRoleAsync(string roleId)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/access/roles/{roleId}");
            var (_, error) = await ParseApiResponseAsync<string>(response);
            return error == null ? (true, null) : (false, error);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(ManagedUserListResponse? List, string? Error)> GetUsersAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        string? userType = null,
        string? roleFilter = null)
    {
        try
        {
            var query = new List<string>
            {
                $"PageNumber={pageNumber}",
                $"PageSize={pageSize}"
            };
            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"Search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(userType))
                query.Add($"UserType={Uri.EscapeDataString(userType)}");
            if (!string.IsNullOrWhiteSpace(roleFilter))
                query.Add($"RoleFilter={Uri.EscapeDataString(roleFilter)}");

            using var response = await Client.GetAsync($"/api/access/users?{string.Join("&", query)}");
            var (data, error) = await ParseApiResponseAsync<ManagedUserListResponse>(response);
            if (error != null)
                return (null, error);

            return data != null ? (data, null) : (null, "No se pudieron cargar los usuarios.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> CreateStaffUserAsync(CreateStaffUserRequest request)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync("/api/access/users/staff", request);
            var (_, error) = await ParseApiResponseAsync<object>(response);
            return error == null ? (true, null) : (false, error);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> AssignUserRolesAsync(string userId, List<string> roleNames)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync(
                $"/api/access/users/{userId}/roles",
                new AssignUserRolesRequest { RoleNames = roleNames });

            var (_, error) = await ParseApiResponseAsync<string>(response);
            return error == null ? (true, null) : (false, error);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> SetUserLockoutAsync(string userId, bool lockoutEnabled)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync(
                $"/api/access/users/{userId}/lockout",
                new SetUserLockoutRequest { LockoutEnabled = lockoutEnabled });

            var (_, error) = await ParseApiResponseAsync<string>(response);
            return error == null ? (true, null) : (false, error);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private static async Task<(T? Data, string? Error)> ParseApiResponseAsync<T>(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return (default, SessionExpiredMessage);

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return (default, ForbiddenMessage);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            return (default, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

        var raw = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (!response.IsSuccessStatusCode)
                return (default, InvalidResponseMessage);

            return (default, "No se pudo completar la operación.");
        }

        ApiResponse<T>? body;
        try
        {
            body = JsonSerializer.Deserialize<ApiResponse<T>>(raw, JsonOptions);
        }
        catch (JsonException)
        {
            return (default, InvalidResponseMessage);
        }

        if (body?.Succeeded == true)
            return (body.Data, null);

        if (body?.Message != null)
            return (default, body.Message);

        if (!response.IsSuccessStatusCode)
            return (default, InvalidResponseMessage);

        return (default, "No se pudo completar la operación.");
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;
}
