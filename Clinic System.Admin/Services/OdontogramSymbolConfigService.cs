using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clinic_System.Core.Odontogram;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public sealed class OdontogramSymbolConfigState
{
    private readonly object _gate = new();
    private OdontogramSymbolConfigDocument _current = OdontogramSymbolDefaults.Create();
    private bool _loaded;

    public event Action? Changed;

    public OdontogramSymbolConfigDocument Current
    {
        get
        {
            lock (_gate)
                return _current;
        }
    }

    public bool Loaded
    {
        get
        {
            lock (_gate)
                return _loaded;
        }
    }

    public void Set(OdontogramSymbolConfigDocument document)
    {
        lock (_gate)
        {
            _current = OdontogramSymbolDefaults.Merge(document);
            _loaded = true;
        }

        Changed?.Invoke();
    }
}

public sealed class OdontogramSymbolConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;
    private readonly OdontogramSymbolConfigState _state;

    public OdontogramSymbolConfigService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings,
        OdontogramSymbolConfigState state)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
        _state = state;
    }

    public OdontogramSymbolConfigDocument Current => _state.Current;

    public event Action? Changed
    {
        add => _state.Changed += value;
        remove => _state.Changed -= value;
    }

    public async Task<(OdontogramSymbolConfigDocument Config, string? Error)> GetAsync(bool forceRefresh = false)
    {
        if (_state.Loaded && !forceRefresh)
            return (_state.Current, null);

        try
        {
            using var response = await _apiClient.Client.GetAsync("/api/clinic/odontogram/symbol-config");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (_state.Current, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (_state.Current, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<OdontogramSymbolConfigDocument>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
            {
                _state.Set(body.Data);
                return (_state.Current, null);
            }

            return (_state.Current, body?.Message ?? "No se pudo cargar la simbología del odontograma.");
        }
        catch (Exception ex)
        {
            return (_state.Current, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(OdontogramSymbolConfigDocument? Config, string? Error)> SaveAsync(OdontogramSymbolConfigDocument document)
    {
        try
        {
            using var response = await _apiClient.Client.PutAsJsonAsync(
                "/api/clinic/odontogram/symbol-config",
                document,
                JsonOptions);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return (null, "No tiene permiso para modificar la simbología.");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<OdontogramSymbolConfigDocument>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
            {
                _state.Set(body.Data);
                return (_state.Current, null);
            }

            return (null, FormatApiError(body, "No se pudo guardar la simbología."));
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(OdontogramSymbolConfigDocument? Config, string? Error)> RestoreAsync()
    {
        try
        {
            using var response = await _apiClient.Client.PostAsync(
                "/api/clinic/odontogram/symbol-config/restore",
                null);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return (null, "No tiene permiso para restaurar la simbología.");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<OdontogramSymbolConfigDocument>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
            {
                _state.Set(body.Data);
                return (_state.Current, null);
            }

            return (null, FormatApiError(body, "No se pudo restaurar la simbología."));
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private static string FormatApiError<T>(ApiResponse<T>? body, string fallback) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message ?? fallback;
}
