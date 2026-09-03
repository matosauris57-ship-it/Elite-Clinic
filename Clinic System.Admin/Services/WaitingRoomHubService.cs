using DentalCare.Admin.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public sealed class WaitingRoomHubService : IAsyncDisposable
{
    private readonly ApiSettings _apiSettings;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiTokenRefreshService _tokenRefresh;
    private readonly object _gate = new();
    private HubConnection? _connection;
    private bool _started;

    public WaitingRoomConnectionState State { get; private set; } = WaitingRoomConnectionState.Disconnected;
    public string? LastError { get; private set; }

    public event Action? StateChanged;
    public event Action<WaitingRoomNotification>? NotificationReceived;

    public WaitingRoomHubService(
        IOptions<ApiSettings> apiSettings,
        TokenStorage tokenStorage,
        ApiTokenRefreshService tokenRefresh)
    {
        _apiSettings = apiSettings.Value;
        _tokenStorage = tokenStorage;
        _tokenRefresh = tokenRefresh;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLoadedTokenAsync();

        if (string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
        {
            LastError = "No hay sesión activa. Inicia sesión en el Admin antes de abrir la sala de espera.";
            SetState(WaitingRoomConnectionState.Faulted);
            return;
        }

        lock (_gate)
        {
            if (_connection is null)
                _connection = BuildConnection();
        }

        if (_connection.State == HubConnectionState.Connected)
        {
            SetState(WaitingRoomConnectionState.Connected);
            return;
        }

        SetState(WaitingRoomConnectionState.Connecting);
        LastError = null;

        try
        {
            if (!_started)
            {
                await _connection.StartAsync(cancellationToken);
                _started = true;
            }
            else if (_connection.State != HubConnectionState.Connected)
            {
                await _connection.StartAsync(cancellationToken);
            }

            await _connection.InvokeAsync("JoinWaitingRoom", cancellationToken);
            SetState(WaitingRoomConnectionState.Connected);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            SetState(WaitingRoomConnectionState.Faulted);
        }
    }

    public async Task StopAsync()
    {
        HubConnection? connection;
        lock (_gate)
        {
            connection = _connection;
            _connection = null;
            _started = false;
        }

        if (connection is null)
        {
            SetState(WaitingRoomConnectionState.Disconnected);
            return;
        }

        try
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
        catch
        {
            // ignore shutdown errors
        }

        SetState(WaitingRoomConnectionState.Disconnected);
    }

    public async ValueTask DisposeAsync() => await StopAsync();

    private HubConnection BuildConnection()
    {
        var hubUrl = $"{_apiSettings.ApiBaseUrl.TrimEnd('/')}/hubs/notifications";

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    await EnsureLoadedTokenAsync();
                    await _tokenRefresh.TryRefreshAsync();
                    return _tokenStorage.AccessToken;
                };

                if (_apiSettings.ApiBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        connection.On<WaitingRoomNotification>("ReceiveNotification", notification =>
        {
            if (notification is null)
                return;

            // Este grupo solo recibe llamados de pacientes (CallPatientCommand).
            NotificationReceived?.Invoke(notification);
        });

        connection.Reconnecting += _ =>
        {
            SetState(WaitingRoomConnectionState.Reconnecting);
            return Task.CompletedTask;
        };

        connection.Reconnected += async _ =>
        {
            try
            {
                await connection.InvokeAsync("JoinWaitingRoom");
                LastError = null;
                SetState(WaitingRoomConnectionState.Connected);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                SetState(WaitingRoomConnectionState.Faulted);
            }
        };

        connection.Closed += error =>
        {
            _started = false;
            LastError = error?.Message;
            SetState(WaitingRoomConnectionState.Disconnected);
            return Task.CompletedTask;
        };

        return connection;
    }

    private async Task EnsureLoadedTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_tokenStorage.AccessToken))
            await _tokenStorage.LoadAsync();
    }

    private void SetState(WaitingRoomConnectionState state)
    {
        State = state;
        StateChanged?.Invoke();
    }
}
