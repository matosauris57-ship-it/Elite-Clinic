using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;

namespace DentalCare.Admin.Services;

public class EmailCampaignMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;

    public EmailCampaignMaintenanceService(AuthenticatedApiClient apiClient, TokenStorage tokenStorage)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(EmailCampaignAudience? Data, string? Error)> GetAudienceAsync()
        => await GetAsync<EmailCampaignAudience>("/api/email-campaigns/audience");

    public async Task<(List<EmailCampaignListItem> Data, string? Error)> ListAsync()
    {
        var (data, error) = await GetAsync<List<EmailCampaignListItem>>("/api/email-campaigns");
        return (data ?? [], error);
    }

    public Task<(EmailCampaignDetail? Data, string? Error)> GetAsync(int id)
        => GetAsync<EmailCampaignDetail>($"/api/email-campaigns/{id}");

    public Task<(EmailCampaignDetail? Data, string? Error)> CreateAsync(EmailCampaignForm form)
        => SendAsync<EmailCampaignDetail>(HttpMethod.Post, "/api/email-campaigns", form);

    public Task<(EmailCampaignDetail? Data, string? Error)> UpdateDraftAsync(int id, EmailCampaignForm form)
        => SendAsync<EmailCampaignDetail>(HttpMethod.Put, $"/api/email-campaigns/{id}", form);

    public Task<(EmailCampaignDetail? Data, string? Error)> StartAsync(int id)
        => SendAsync<EmailCampaignDetail>(HttpMethod.Post, $"/api/email-campaigns/{id}/start", null);

    public Task<(EmailCampaignDetail? Data, string? Error)> PauseAsync(int id)
        => SendAsync<EmailCampaignDetail>(HttpMethod.Post, $"/api/email-campaigns/{id}/pause", null);

    public Task<(EmailCampaignDetail? Data, string? Error)> ResumeAsync(int id)
        => SendAsync<EmailCampaignDetail>(HttpMethod.Post, $"/api/email-campaigns/{id}/resume", null);

    public Task<(EmailCampaignDetail? Data, string? Error)> CancelAsync(int id)
        => SendAsync<EmailCampaignDetail>(HttpMethod.Post, $"/api/email-campaigns/{id}/cancel", null);

    private async Task<(T? Data, string? Error)> GetAsync<T>(string url)
    {
        try
        {
            using var response = await Client.GetAsync(url);
            return await ReadAsync<T>(response);
        }
        catch (Exception ex)
        {
            return (default, ex.Message);
        }
    }

    private async Task<(T? Data, string? Error)> SendAsync<T>(HttpMethod method, string url, object? body)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            if (body != null)
                request.Content = JsonContent.Create(body, options: JsonOptions);
            using var response = await Client.SendAsync(request);
            return await ReadAsync<T>(response);
        }
        catch (Exception ex)
        {
            return (default, ex.Message);
        }
    }

    private async Task<(T? Data, string? Error)> ReadAsync<T>(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return (default, ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
        if (ApiConnectionMessages.IsRateLimited(response))
            return (default, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        if (payload?.Succeeded == true)
            return (payload.Data, null);
        return (default, payload?.Message ?? "No se pudo completar la operación.");
    }
}
