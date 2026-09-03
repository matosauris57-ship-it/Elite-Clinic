using Microsoft.AspNetCore.Components.Server.Circuits;

namespace DentalCare.Admin.Services;

public class SessionCircuitHandler : CircuitHandler
{
    private readonly TokenStorage _tokenStorage;
    private readonly CircuitSessionContext _circuitSession;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SessionReadyService _sessionReady;

    public SessionCircuitHandler(
        TokenStorage tokenStorage,
        CircuitSessionContext circuitSession,
        IHttpContextAccessor httpContextAccessor,
        SessionReadyService sessionReady)
    {
        _tokenStorage = tokenStorage;
        _circuitSession = circuitSession;
        _httpContextAccessor = httpContextAccessor;
        _sessionReady = sessionReady;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        await HydrateTokenAsync();
    }

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
            await HydrateTokenAsync();
    }

    private async Task HydrateTokenAsync()
    {
        if (_sessionReady.IsReady)
            return;

        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                _circuitSession.User = httpContext.User;
                await _tokenStorage.LoadFromHttpContextAsync(httpContext);
                _tokenStorage.EnsureRefreshTokenHydrated(httpContext.User);
            }
            else if (_circuitSession.User != null)
            {
                await _tokenStorage.LoadAsync(_circuitSession.User);
                _tokenStorage.EnsureRefreshTokenHydrated(_circuitSession.User);
            }
            else
            {
                _tokenStorage.SyncFromPersistence();
            }
        }
        finally
        {
            if (!_sessionReady.IsReady)
                _sessionReady.MarkReady(hasToken: !string.IsNullOrWhiteSpace(_tokenStorage.AccessToken));
        }
    }
}
