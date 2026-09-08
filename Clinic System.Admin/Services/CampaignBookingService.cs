using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class CampaignBookingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _apiSettings;

    public CampaignBookingService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _httpClientFactory.CreateClient("ClinicApiLogin");

    public async Task<(CampaignBookingContext? Data, string? Error)> GetContextAsync(string token)
    {
        try
        {
            var url = $"/api/campaign-booking/context?token={Uri.EscapeDataString(token)}";
            var body = await Client.GetFromJsonAsync<ApiResponse<CampaignBookingContext>>(url, JsonOptions);
            return body?.Succeeded == true && body.Data != null
                ? (body.Data, null)
                : (null, body?.Message ?? "El enlace no es válido o expiró.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<string> Occupied, string? Error)> GetOccupiedAsync(string token, int doctorId, DateTime date)
    {
        try
        {
            var url = $"/api/campaign-booking/occupied?token={Uri.EscapeDataString(token)}&doctorId={doctorId}&date={date:yyyy-MM-dd}";
            var body = await Client.GetFromJsonAsync<ApiResponse<List<string>>>(url, JsonOptions);
            return body?.Succeeded == true
                ? (body.Data ?? [], null)
                : ([], body?.Message ?? "No se pudieron cargar los horarios ocupados.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(CampaignBookingResult? Data, string? Error)> BookAsync(CampaignBookFormRequest request)
    {
        try
        {
            using var response = await Client.PostAsJsonAsync("/api/campaign-booking/book", request, JsonOptions);
            var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignBookingResult>>(JsonOptions);
            return body?.Succeeded == true
                ? (body.Data, null)
                : (null, body?.Message ?? "No se pudo agendar la cita.");
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
}
