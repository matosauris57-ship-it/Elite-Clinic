using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class AuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStorage _tokenStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly SessionReadyService _sessionReady;
    private readonly ApiTokenRefreshService _tokenRefresh;
    private readonly ApiSettings _apiSettings;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        TokenStorage tokenStorage,
        AuthenticationStateProvider authStateProvider,
        IMemoryCache memoryCache,
        SessionReadyService sessionReady,
        ApiTokenRefreshService tokenRefresh,
        IOptions<ApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStorage = tokenStorage;
        _authStateProvider = authStateProvider;
        _memoryCache = memoryCache;
        _sessionReady = sessionReady;
        _tokenRefresh = tokenRefresh;
        _apiSettings = apiSettings.Value;
    }

    public async Task SaveSessionAsync(LoginResponse data)
    {
        await _tokenStorage.SaveAsync(data);

        if (_authStateProvider is CustomAuthStateProvider customProvider)
        {
            customProvider.NotifyAuthenticationStateChanged();
        }
    }

    public async Task<(bool Success, string? Error, string? BootstrapKey, LoginResponse? Data)> LoginAsync(string email, string password)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ClinicApiLogin");
            var response = await client.PostAsJsonAsync("/api/authentication/login", new LoginRequest
            {
                EmailOrUserName = email,
                Password = password,
                ForAdminPanel = true
            });

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 429)
                    return (false, ApiConnectionMessages.RateLimitedMessage, null, null);

                var error = await TryReadErrorAsync(response);
                return (false, error ?? "Credenciales inválidas.", null, null);
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(JsonOptions);
            if (apiResponse?.Succeeded != true || apiResponse.Data == null || string.IsNullOrWhiteSpace(apiResponse.Data.AccessToken))
            {
                return (false, apiResponse?.Message ?? "No se pudo iniciar sesión.", null, null);
            }

            var data = apiResponse.Data;
            var bootstrapKey = Guid.NewGuid().ToString("N");
            _memoryCache.Set(bootstrapKey, new SessionBootstrapData
            {
                AccessToken = data.AccessToken,
                RefreshToken = data.RefreshToken,
                ExpiresAt = data.ExpiresAt,
                UserName = data.UserName,
                Email = data.Email,
                Roles = data.Roles,
                Permissions = data.Permissions
            }, TimeSpan.FromMinutes(2));

            return (true, null, bootstrapKey, data);
        }
        catch (Exception ex) when (ApiConnectionMessages.IsConnectionFailure(ex))
        {
            return (false, ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl), null, null);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Sesión no lista. Espera un momento y vuelve a intentar.", null, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error al iniciar sesión: {ex.Message}", null, null);
        }
    }

    public async Task LogoutAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        await _tokenStorage.ClearAsync(authState.User);

        if (_authStateProvider is CustomAuthStateProvider customProvider)
        {
            customProvider.NotifyAuthenticationStateChanged();
        }
    }

    public async Task<bool> EnsureSessionLoadedAsync()
    {
        await _sessionReady.WaitUntilReadyAsync();

        if (!string.IsNullOrWhiteSpace(_tokenStorage.AccessToken) && !string.IsNullOrWhiteSpace(_tokenStorage.RefreshToken))
            return true;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            await _tokenStorage.LoadAsync(authState.User);
            _tokenStorage.EnsureRefreshTokenHydrated(authState.User);
            _tokenStorage.SyncFromPersistence(authState.User);
            return !string.IsNullOrWhiteSpace(_tokenStorage.AccessToken);
        }

        if (!string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
        {
            _tokenStorage.EnsureRefreshTokenHydrated();
            _tokenStorage.SyncFromPersistence();
            return true;
        }

        return false;
    }

    public async Task<bool> TryRefreshSessionAsync()
    {
        await EnsureSessionLoadedAsync();
        return await _tokenRefresh.TryRefreshAsync(force: true);
    }

    public async Task<(T Data, string? Error)> WithTokenRetryAsync<T>(
        Func<Task<(T Data, string? Error)>> action,
        Func<string?, bool> isTokenError)
    {
        await EnsureSessionLoadedAsync();
        var (data, error) = await action();

        if (!isTokenError(error))
            return (data, error);

        if (!await TryRefreshSessionAsync())
            return (data, error);

        return await action();
    }

    public static bool IsTokenError(string? error) =>
        !string.IsNullOrWhiteSpace(error) &&
        (error.Contains("token", StringComparison.OrdinalIgnoreCase) ||
         error.Contains("sesión", StringComparison.OrdinalIgnoreCase));

    private static async Task<string?> TryReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            return apiResponse?.Message;
        }
        catch
        {
            return null;
        }
    }
}
