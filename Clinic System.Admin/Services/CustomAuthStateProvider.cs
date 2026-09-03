using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace DentalCare.Admin.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CircuitSessionContext _circuitSession;
    private AuthenticationState? _cachedState;

    public CustomAuthStateProvider(
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor,
        CircuitSessionContext circuitSession)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        _circuitSession = circuitSession;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedState?.User.Identity?.IsAuthenticated == true)
        {
            _circuitSession.User ??= _cachedState.User;
            await EnsureTokenLoadedAsync(_cachedState.User);
            return _cachedState;
        }

        var httpUser = _httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
        {
            _circuitSession.User = httpUser;
            await EnsureTokenLoadedAsync(httpUser);
            _cachedState = new AuthenticationState(httpUser);
            return _cachedState;
        }

        var storage = _serviceProvider.GetRequiredService<TokenStorage>();
        await storage.LoadAsync();

        if (!string.IsNullOrWhiteSpace(storage.AccessToken))
        {
            _cachedState = new AuthenticationState(CreatePrincipal(
                storage.UserName ?? "Admin",
                storage.Email ?? string.Empty,
                storage.Roles,
                storage.Permissions,
                storage.CachedSessionId,
                storage.AccessToken));
            return _cachedState;
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public void NotifyAuthenticationStateChanged()
    {
        _cachedState = null;
        _circuitSession.User = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task EnsureTokenLoadedAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
            return;

        var tokenStorage = _serviceProvider.GetRequiredService<TokenStorage>();
        if (string.IsNullOrWhiteSpace(tokenStorage.AccessToken))
            await tokenStorage.LoadAsync(user);

        var sessionReady = _serviceProvider.GetRequiredService<SessionReadyService>();
        if (!sessionReady.IsReady)
            sessionReady.MarkReady(hasToken: !string.IsNullOrWhiteSpace(tokenStorage.AccessToken));
    }

    private static ClaimsPrincipal CreatePrincipal(
        string userName,
        string email,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        string? sessionId = null,
        string? accessToken = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email)
        };

        if (!string.IsNullOrWhiteSpace(sessionId))
            claims.Add(new Claim(TokenStorage.SessionClaimType, sessionId));

        if (!string.IsNullOrWhiteSpace(accessToken))
            claims.Add(new Claim(TokenStorage.ApiTokenClaimType, accessToken));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (permissions != null)
        {
            foreach (var permission in permissions)
                claims.Add(new Claim(PermissionService.ClaimType, permission));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "jwt"));
    }
}
