using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DentalCare.Admin.Services;

public class TokenStorage
{
    public const string SessionClaimType = "dentalcare_session_id";
    public const string ApiTokenClaimType = "dentalcare_api_token";

    private readonly IMemoryCache _memoryCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiSessionCookieService _apiSessionCookie;
    private readonly CircuitSessionContext _circuitSession;

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public IReadOnlyList<string> Roles => _roles;
    public IReadOnlyList<string> Permissions => _permissions;
    private List<string> _roles = [];
    private List<string> _permissions = [];
    private string? _cachedSessionId;

    public TokenStorage(
        IMemoryCache memoryCache,
        IHttpContextAccessor httpContextAccessor,
        ApiSessionCookieService apiSessionCookie,
        CircuitSessionContext circuitSession)
    {
        _memoryCache = memoryCache;
        _httpContextAccessor = httpContextAccessor;
        _apiSessionCookie = apiSessionCookie;
        _circuitSession = circuitSession;
    }

    public static string GetSessionCacheKey(string sessionId) => $"admin_session_{sessionId}";

    public string? CachedSessionId => _cachedSessionId;

    public void ApplyPersistedSession(SessionBootstrapData data)
    {
        ApplyData(data);
        if (!string.IsNullOrWhiteSpace(data.SessionId))
            RememberSessionId(data.SessionId);
    }

    public bool TryLoadFromMemoryCache(string? sessionId = null)
    {
        sessionId ??= _cachedSessionId;
        return !string.IsNullOrWhiteSpace(sessionId) && TryLoadFromCache(sessionId);
    }

    public Task SaveAsync(LoginResponse data)
    {
        ApplyData(new SessionBootstrapData
        {
            AccessToken = data.AccessToken,
            RefreshToken = data.RefreshToken,
            ExpiresAt = data.ExpiresAt,
            UserName = data.UserName,
            Email = data.Email,
            Roles = data.Roles,
            Permissions = data.Permissions
        });
        return Task.CompletedTask;
    }

    public Task<bool> LoadAsync(ClaimsPrincipal? user = null)
    {
        if (!string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(RefreshToken))
            return Task.FromResult(true);

        user ??= _httpContextAccessor.HttpContext?.User;

        var sessionId = ResolveSessionId(user);
        if (!string.IsNullOrWhiteSpace(sessionId) && TryLoadFromCache(sessionId))
            return Task.FromResult(true);

        if (TryLoadFromCookie())
            return Task.FromResult(true);

        if (TryLoadFromClaims(user))
        {
            EnsureRefreshTokenHydrated(user);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<bool> LoadFromHttpContextAsync(HttpContext httpContext)
    {
        if (!string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(RefreshToken))
            return Task.FromResult(true);

        var user = httpContext.User;
        var sessionId = user.FindFirst(SessionClaimType)?.Value;
        if (!string.IsNullOrWhiteSpace(sessionId))
            RememberSessionId(sessionId);

        if (!string.IsNullOrWhiteSpace(sessionId) && TryLoadFromCache(sessionId))
            return Task.FromResult(true);

        var cookieData = _apiSessionCookie.TryRead(httpContext);
        if (cookieData != null)
        {
            ApplyData(cookieData);
            return Task.FromResult(true);
        }

        return LoadAsync(user);
    }

    public void EnsureRefreshTokenHydrated(ClaimsPrincipal? user = null)
    {
        if (!string.IsNullOrWhiteSpace(RefreshToken))
            return;

        user ??= _httpContextAccessor.HttpContext?.User;
        var sessionId = ResolveSessionId(user);

        if (!string.IsNullOrWhiteSpace(sessionId) && TryMergeFromCache(sessionId))
            return;

        TryMergeFromCookie();
    }

    public void RememberSessionId(string sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
            _cachedSessionId = sessionId;
    }

    public bool IsAccessTokenExpiringSoon(TimeSpan threshold)
    {
        var expiresAt = ResolveExpiresAtUtc();
        if (!expiresAt.HasValue)
            return false;

        return expiresAt.Value <= DateTime.UtcNow.Add(threshold);
    }

    public bool IsAccessTokenExpired() => IsAccessTokenExpiringSoon(TimeSpan.Zero);

    public DateTime? ResolveExpiresAtUtc() => ExpiresAtUtc ?? TryParseJwtExpiry(AccessToken);

    public void UpdateTokens(string accessToken, string refreshToken, string? expiresAt, IEnumerable<string>? permissions = null)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAtUtc = ParseExpiresAt(expiresAt);
        if (permissions != null)
            _permissions = permissions.ToList();
    }

    public void SyncFromPersistence(ClaimsPrincipal? user = null)
    {
        user ??= _httpContextAccessor.HttpContext?.User ?? _circuitSession.User;
        var sessionId = ResolveSessionId(user);

        if (!string.IsNullOrWhiteSpace(sessionId) &&
            _memoryCache.TryGetValue<SessionBootstrapData>(GetSessionCacheKey(sessionId), out var cached) &&
            cached != null &&
            !string.IsNullOrWhiteSpace(cached.AccessToken))
        {
            ApplyData(cached);
            return;
        }

        TryMergeFromCookie();
    }

    public void PersistSession()
    {
        var sessionId = ResolveSessionId(_circuitSession.User);
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        var data = ToSessionData();
        _memoryCache.Set(GetSessionCacheKey(sessionId), data, TimeSpan.FromHours(8));
        _apiSessionCookie.Write(data);
    }

    public SessionBootstrapData ToSessionData() => new()
    {
        SessionId = _cachedSessionId ?? string.Empty,
        AccessToken = AccessToken ?? string.Empty,
        RefreshToken = RefreshToken ?? string.Empty,
        ExpiresAt = ExpiresAtUtc?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty,
        UserName = UserName ?? string.Empty,
        Email = Email ?? string.Empty,
        Roles = _roles.ToList(),
        Permissions = _permissions.ToList()
    };

    public Task ClearAsync(ClaimsPrincipal? user = null)
    {
        var sessionId = ResolveSessionId(user);

        if (!string.IsNullOrWhiteSpace(sessionId))
            _memoryCache.Remove(GetSessionCacheKey(sessionId));

        _apiSessionCookie.Delete();

        AccessToken = null;
        RefreshToken = null;
        ExpiresAtUtc = null;
        UserName = null;
        Email = null;
        _roles = [];
        _permissions = [];
        _cachedSessionId = null;
        return Task.CompletedTask;
    }

    private string? ResolveSessionId(ClaimsPrincipal? user)
    {
        var sessionId =
            _httpContextAccessor.HttpContext?.User?.FindFirst(SessionClaimType)?.Value
            ?? user?.FindFirst(SessionClaimType)?.Value
            ?? _circuitSession.User?.FindFirst(SessionClaimType)?.Value
            ?? _cachedSessionId;

        if (!string.IsNullOrWhiteSpace(sessionId))
            _cachedSessionId = sessionId;

        return sessionId;
    }

    private bool TryLoadFromCache(string sessionId)
    {
        if (!_memoryCache.TryGetValue<SessionBootstrapData>(GetSessionCacheKey(sessionId), out var data) || data == null)
            return false;

        ApplyData(data);
        return true;
    }

    private bool TryLoadFromCookie()
    {
        var cookieData = _apiSessionCookie.TryRead();
        if (cookieData == null)
            return false;

        ApplyData(cookieData);
        return true;
    }

    private bool TryLoadFromClaims(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        var token = user.FindFirst(ApiTokenClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        AccessToken = token;
        UserName = user.FindFirst(ClaimTypes.Name)?.Value;
        Email = user.FindFirst(ClaimTypes.Email)?.Value;
        _roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        _permissions = user.FindAll(PermissionService.ClaimType).Select(c => c.Value).ToList();
        var sessionId = user.FindFirst(SessionClaimType)?.Value;
        if (!string.IsNullOrWhiteSpace(sessionId))
            RememberSessionId(sessionId);

        EnsureRefreshTokenHydrated(user);
        return true;
    }

    private bool TryMergeFromCache(string sessionId)
    {
        if (!_memoryCache.TryGetValue<SessionBootstrapData>(GetSessionCacheKey(sessionId), out var data) || data == null)
            return false;

        MergePersistedData(data);
        return !string.IsNullOrWhiteSpace(RefreshToken);
    }

    private bool TryMergeFromCookie()
    {
        var cookieData = _apiSessionCookie.TryRead();
        if (cookieData == null)
            return false;

        MergePersistedData(cookieData);
        return !string.IsNullOrWhiteSpace(RefreshToken);
    }

    private void MergePersistedData(SessionBootstrapData data)
    {
        if (string.IsNullOrWhiteSpace(RefreshToken) && !string.IsNullOrWhiteSpace(data.RefreshToken))
            RefreshToken = data.RefreshToken;

        if (!ExpiresAtUtc.HasValue && !string.IsNullOrWhiteSpace(data.ExpiresAt))
            ExpiresAtUtc = ParseExpiresAt(data.ExpiresAt);

        if (string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(data.AccessToken))
            AccessToken = data.AccessToken;

        if (string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(data.UserName))
            UserName = data.UserName;

        if (string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(data.Email))
            Email = data.Email;

        if (_roles.Count == 0 && data.Roles.Count > 0)
            _roles = data.Roles;

        if (_permissions.Count == 0 && data.Permissions.Count > 0)
            _permissions = data.Permissions;

        if (!string.IsNullOrWhiteSpace(data.SessionId))
            RememberSessionId(data.SessionId);
    }

    private static DateTime? TryParseJwtExpiry(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return null;

        try
        {
            var parts = accessToken.Split('.');
            if (parts.Length < 2)
                return null;

            var payload = parts[1];
            var padding = payload.Length % 4;
            if (padding > 0)
                payload += new string('=', 4 - padding);

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("exp", out var expElement))
                return null;

            return DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64()).UtcDateTime;
        }
        catch
        {
            return null;
        }
    }

    private void ApplyData(SessionBootstrapData data)
    {
        AccessToken = data.AccessToken;
        RefreshToken = data.RefreshToken;
        ExpiresAtUtc = ParseExpiresAt(data.ExpiresAt);
        UserName = data.UserName;
        Email = data.Email;
        _roles = data.Roles;
        _permissions = data.Permissions;
        if (!string.IsNullOrWhiteSpace(data.SessionId))
            RememberSessionId(data.SessionId);
    }

    private static DateTime? ParseExpiresAt(string? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(expiresAt))
            return null;

        if (DateTime.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utc))
            return utc;

        if (DateTime.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var local))
            return local.ToUniversalTime();

        return null;
    }
}
