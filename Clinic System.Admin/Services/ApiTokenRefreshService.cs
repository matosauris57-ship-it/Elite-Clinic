using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;

namespace DentalCare.Admin.Services;

public class ApiTokenRefreshService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly TimeSpan ExpiryThreshold = TimeSpan.FromMinutes(5);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStorage _tokenStorage;
    private readonly CircuitSessionContext _circuitSession;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public ApiTokenRefreshService(
        IHttpClientFactory httpClientFactory,
        TokenStorage tokenStorage,
        CircuitSessionContext circuitSession)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStorage = tokenStorage;
        _circuitSession = circuitSession;
    }

    public bool ShouldRefreshProactively() =>
        !string.IsNullOrWhiteSpace(_tokenStorage.RefreshToken) &&
        _tokenStorage.IsAccessTokenExpiringSoon(ExpiryThreshold);

    public async Task<bool> TryRefreshAsync(bool force = false)
    {
        _tokenStorage.SyncFromPersistence(_circuitSession.User);

        if (string.IsNullOrWhiteSpace(_tokenStorage.RefreshToken))
            return false;

        if (!force && !ShouldRefreshProactively())
            return !string.IsNullOrWhiteSpace(_tokenStorage.AccessToken);

        await _refreshLock.WaitAsync();
        try
        {
            _tokenStorage.SyncFromPersistence(_circuitSession.User);

            if (!force && !ShouldRefreshProactively())
                return !string.IsNullOrWhiteSpace(_tokenStorage.AccessToken);

            var previousRefresh = _tokenStorage.RefreshToken;
            if (await ExecuteRefreshAsync())
                return true;

            _tokenStorage.SyncFromPersistence(_circuitSession.User);
            if (!string.Equals(previousRefresh, _tokenStorage.RefreshToken, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(_tokenStorage.RefreshToken))
            {
                return await ExecuteRefreshAsync();
            }

            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<bool> ExecuteRefreshAsync()
    {
        if (string.IsNullOrWhiteSpace(_tokenStorage.RefreshToken))
            return false;

        try
        {
            var client = _httpClientFactory.CreateClient("ClinicApiLogin");
            var response = await client.PostAsJsonAsync("/api/authentication/refresh-token", new RefreshTokenRequest
            {
                AccessToken = _tokenStorage.AccessToken ?? string.Empty,
                RefreshToken = _tokenStorage.RefreshToken
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<JwtAuthResult>>(JsonOptions);
            if (body?.Succeeded != true || body.Data == null || string.IsNullOrWhiteSpace(body.Data.AccessToken))
                return false;

            _tokenStorage.UpdateTokens(body.Data.AccessToken, body.Data.RefreshToken, body.Data.ExpiresAt, body.Data.Permissions);
            _tokenStorage.PersistSession();
            return true;
        }
        catch (Exception ex) when (ApiConnectionMessages.IsConnectionFailure(ex) || ex is TaskCanceledException or TimeoutException)
        {
            return false;
        }
    }
}
