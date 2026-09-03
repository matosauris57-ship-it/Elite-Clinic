using System.Net.Http.Headers;

namespace DentalCare.Admin.Services;

public class ApiAuthHandler : DelegatingHandler
{
    private readonly TokenStorage _tokenStorage;
    private readonly CircuitSessionContext _circuitSession;
    private readonly SessionReadyService _sessionReady;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiTokenRefreshService _tokenRefresh;

    public ApiAuthHandler(
        TokenStorage tokenStorage,
        CircuitSessionContext circuitSession,
        SessionReadyService sessionReady,
        IHttpContextAccessor httpContextAccessor,
        ApiTokenRefreshService tokenRefresh)
    {
        _tokenStorage = tokenStorage;
        _circuitSession = circuitSession;
        _sessionReady = sessionReady;
        _httpContextAccessor = httpContextAccessor;
        _tokenRefresh = tokenRefresh;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_sessionReady.IsReady)
            await EnsureSessionReadyAsync();

        await EnsureAccessTokenAsync();
        _tokenStorage.EnsureRefreshTokenHydrated(_circuitSession.User);
        _tokenStorage.SyncFromPersistence(_circuitSession.User);

        if (!string.IsNullOrWhiteSpace(_tokenStorage.RefreshToken) &&
            (_tokenRefresh.ShouldRefreshProactively() || _tokenStorage.IsAccessTokenExpired()))
            await _tokenRefresh.TryRefreshAsync(force: false);

        ApplyAuthorizationHeader(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized &&
            await _tokenRefresh.TryRefreshAsync(force: true))
        {
            response.Dispose();
            using var retryRequest = await CloneRequestAsync(request, cancellationToken);
            ApplyAuthorizationHeader(retryRequest);
            return await base.SendAsync(retryRequest, cancellationToken);
        }

        return response;
    }

    private async Task EnsureSessionReadyAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            _circuitSession.User ??= httpContext.User;
            await _tokenStorage.LoadFromHttpContextAsync(httpContext);
            _tokenStorage.EnsureRefreshTokenHydrated(httpContext.User);
            _sessionReady.MarkReady(hasToken: !string.IsNullOrWhiteSpace(_tokenStorage.AccessToken));
            return;
        }

        await _tokenStorage.LoadAsync(_circuitSession.User);
        if (string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
        {
            var sessionId = _circuitSession.User?.FindFirst(TokenStorage.SessionClaimType)?.Value
                ?? _tokenStorage.CachedSessionId;
            _tokenStorage.TryLoadFromMemoryCache(sessionId);
        }

        if (!string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
        {
            _sessionReady.MarkReady(hasToken: true);
            return;
        }

        await _sessionReady.WaitUntilReadyAsync();
    }

    private async Task EnsureAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
            return;

        await _tokenStorage.LoadAsync(_circuitSession.User);

        if (string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
        {
            var sessionId = _circuitSession.User?.FindFirst(TokenStorage.SessionClaimType)?.Value
                ?? _tokenStorage.CachedSessionId;
            _tokenStorage.TryLoadFromMemoryCache(sessionId);
        }

        _tokenStorage.EnsureRefreshTokenHydrated(_circuitSession.User);
    }

    private void ApplyAuthorizationHeader(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenStorage.AccessToken);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                continue;

            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
