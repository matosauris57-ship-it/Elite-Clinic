using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class ToothChartService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public ToothChartService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<ToothChartEntry> Entries, string? Error)> GetChartAsync(
        int patientId,
        string? dentition = null,
        int? quadrant = null)
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(dentition))
                query.Add($"dentition={Uri.EscapeDataString(dentition)}");
            if (quadrant.HasValue)
                query.Add($"quadrant={quadrant.Value}");

            var url = $"/api/dental/odontogram/patient/{patientId}/chart";
            if (query.Count > 0)
                url += $"?{string.Join("&", query)}";

            using var response = await Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ToothChartEntry>>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : ([], FormatApiError(body, "No se pudo cargar el odontograma clínico."));
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(ToothChartEntry? Entry, string? Error)> CreateEntryAsync(
        CreateToothChartEntryRequest request)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync(
                "/api/dental/odontogram/entries",
                request,
                JsonOptions);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (null, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<ToothChartEntry>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : (null, FormatApiError(body, "No se pudo registrar el hallazgo."));
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(int Count, string? Error)> SaveForTeethAsync(
        CreateToothChartEntryRequest request,
        IReadOnlyList<int> teeth)
    {
        var numbers = teeth.Distinct().ToList();
        if (numbers.Count == 0)
            return (0, "Seleccione una o más piezas antes de guardar.");

        request.ToothNumber = numbers[0];
        request.ToothNumbers = numbers;

        if (numbers.Count == 1)
        {
            var single = await CreateEntryAsync(request);
            return single.Error == null ? (1, null) : (0, single.Error);
        }

        var batch = await CreateEntriesBatchAsync(ToBatchRequest(request, numbers));
        if (batch.Error == null)
            return (batch.Count > 0 ? batch.Count : numbers.Count, null);

        var created = 0;
        foreach (var tooth in numbers)
        {
            request.ToothNumber = tooth;
            request.ToothNumbers = [tooth];
            var one = await CreateEntryAsync(request);
            if (one.Error != null)
            {
                var detail = created == 0
                    ? one.Error
                    : $"Se guardó en {created} piezas, pero falló en la {tooth}: {one.Error}";
                return (created, detail);
            }

            created++;
        }

        return (created, null);
    }

    public async Task<(int Count, string? Error)> CreateEntriesBatchAsync(CreateToothChartEntryRequest request) =>
        await CreateEntriesBatchAsync(ToBatchRequest(request, request.ToothNumbers));

    public async Task<(int Count, string? Error)> CreateEntriesBatchAsync(CreateToothChartEntriesBatchRequest request)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync(
                "/api/dental/odontogram/entries/batch",
                request,
                JsonOptions);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (0, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return (0, await ApiConnectionMessages.GetRateLimitMessageAsync(response));
            if (!response.IsSuccessStatusCode)
            {
                ApiResponse<List<ToothChartEntry>>? errorBody = null;
                try
                {
                    errorBody = await response.Content.ReadFromJsonAsync<ApiResponse<List<ToothChartEntry>>>(JsonOptions);
                }
                catch (JsonException)
                {
                }

                return (0, FormatApiError(errorBody, "No se pudo registrar el diagnóstico en las piezas."));
            }

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ToothChartEntry>>>(JsonOptions);
            return body?.Succeeded == true
                ? (body.Data?.Count ?? request.ToothNumbers.Count, null)
                : (0, FormatApiError(body, "No se pudo registrar el diagnóstico en las piezas."));
        }
        catch (Exception ex)
        {
            return (0, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<DentalClinicalEvent> Events, string? Error)> GetTimelineAsync(
        int patientId,
        int? toothNumber = null)
    {
        try
        {
            var url = $"/api/dental/odontogram/patient/{patientId}/timeline";
            if (toothNumber.HasValue)
                url += $"?toothNumber={toothNumber.Value}";

            using var response = await Client.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<DentalClinicalEvent>>>(JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : ([], FormatApiError(body, "No se pudo cargar el historial clínico."));
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private static CreateToothChartEntriesBatchRequest ToBatchRequest(
        CreateToothChartEntryRequest request,
        IReadOnlyList<int> teeth) => new()
    {
        PatientId = request.PatientId,
        ToothNumbers = [.. teeth],
        Surface = request.Surface,
        Phase = request.Phase,
        Condition = request.Condition,
        RestorationMaterial = request.RestorationMaterial,
        CariesType = request.CariesType,
        Icdas = request.Icdas,
        Severity = request.Severity,
        ClinicalDiagnosis = request.ClinicalDiagnosis,
        ProposedTreatment = request.ProposedTreatment,
        Notes = request.Notes,
        AppointmentId = request.AppointmentId,
        BridgeSpanId = request.BridgeSpanId,
        BridgeUnits = request.BridgeUnits
    };

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private static string FormatApiError<T>(ApiResponse<T>? body, string fallback) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message ?? fallback;
}
