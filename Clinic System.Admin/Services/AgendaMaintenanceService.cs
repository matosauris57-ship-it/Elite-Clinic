using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class AgendaMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public AgendaMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<AppointmentAgendaItem> Items, string? Error)> GetAgendaAsync(
        DateTime date, int? doctorId = null, string? status = null, DateTime? endDate = null)
    {
        var (page, error) = await GetAgendaPageAsync(date, doctorId, status, endDate);
        return (page.Items?.ToList() ?? [], error);
    }

    public async Task<(PagedResult<AppointmentAgendaItem> Page, string? Error)> GetAgendaPageAsync(
        DateTime? date,
        int? doctorId = null,
        string? status = null,
        DateTime? endDate = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? search = null)
    {
        try
        {
            var query = new List<string>();
            if (date.HasValue)
                query.Add($"Date={date.Value:yyyy-MM-dd}");
            if (endDate.HasValue && (!date.HasValue || endDate.Value.Date != date.Value.Date))
                query.Add($"EndDate={endDate.Value:yyyy-MM-dd}");
            if (doctorId.HasValue)
                query.Add($"DoctorId={doctorId.Value}");
            if (!string.IsNullOrWhiteSpace(status))
                query.Add($"Status={Uri.EscapeDataString(status)}");
            if (pageNumber.HasValue)
                query.Add($"PageNumber={pageNumber.Value}");
            if (pageSize.HasValue)
                query.Add($"PageSize={pageSize.Value}");
            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"Search={Uri.EscapeDataString(search)}");

            var url = query.Count == 0
                ? "/api/appointments/agenda"
                : $"/api/appointments/agenda?{string.Join("&", query)}";
            using var response = await Client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (EmptyAgendaPage(), GetUnauthorizedMessage());

            if (ApiConnectionMessages.IsRateLimited(response))
                return (EmptyAgendaPage(), await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AppointmentAgendaItem>>>(JsonOptions);

            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (EmptyAgendaPage(), body?.Message ?? "No se pudo cargar la agenda.");
        }
        catch (Exception ex)
        {
            return (EmptyAgendaPage(), FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(AppointmentStats? Stats, string? Error)> GetDayStatsAsync(DateTime? date, DateTime? endDate = null)
    {
        try
        {
            var query = new List<string>();
            if (date.HasValue)
                query.Add($"StartDate={date.Value:yyyy-MM-dd}");
            if (endDate.HasValue || date.HasValue)
            {
                var end = (endDate ?? date)!.Value.Date.AddDays(1).AddSeconds(-1);
                query.Add($"EndDate={end:yyyy-MM-dd HH:mm:ss}");
            }

            var url = query.Count == 0
                ? "/api/appointments/stats"
                : $"/api/appointments/stats?{string.Join("&", query)}";
            using var response = await Client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (null, GetUnauthorizedMessage());

            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var stats = await response.Content.ReadFromJsonAsync<AppointmentStats>(JsonOptions);
            return (stats, null);
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> ConfirmAsync(ConfirmAppointmentRequest request)
    {
        try
        {
            var response = await Client.PutAsJsonAsync("/api/appointments/confirm", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> CancelAsync(CancelAppointmentRequest request)
    {
        try
        {
            var response = await Client.PutAsJsonAsync("/api/appointments/cancel", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> RescheduleAsync(RescheduleAppointmentRequest request)
    {
        try
        {
            var response = await Client.PutAsJsonAsync("/api/appointments/reschedule", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(CompleteAppointmentRequest request)
    {
        try
        {
            var response = await Client.PutAsJsonAsync("/api/appointments/complete", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> NoShowAsync(NoShowAppointmentRequest request)
    {
        try
        {
            var response = await Client.PutAsJsonAsync("/api/appointments/noshow", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> CallPatientAsync(int appointmentId)
    {
        try
        {
            var response = await Client.PostAsync($"/api/appointments/call/{appointmentId}", null);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);

            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body));
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private static PagedResult<AppointmentAgendaItem> EmptyAgendaPage() => new();

    private string GetUnauthorizedMessage() =>
        ApiConnectionMessages.UnauthorizedSession(_tokenStorage);

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private static string FormatApiErrors<T>(ApiResponse<T>? body) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message ?? "No se pudo completar la operación.";
}
