using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class BillingMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public BillingMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(PagedResult<PaymentListItem> Page, string? Error)> GetPaymentsAsync(BillingPaymentFilters filters)
    {
        try
        {
            var url = $"/api/payment/list?{BuildPaymentsQuery(filters)}";
            using var response = await Client.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (new PagedResult<PaymentListItem>(), ApiConnectionMessages.UnauthorizedSession(_tokenStorage));

            if (ApiConnectionMessages.IsRateLimited(response))
                return (new PagedResult<PaymentListItem>(), await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<PaymentListItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (new PagedResult<PaymentListItem>(), FormatApiErrors(body) ?? "No se pudieron cargar los pagos.");
        }
        catch (Exception ex)
        {
            return (new PagedResult<PaymentListItem>(), FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(DailyRevenue? Revenue, string? Error)> GetDailyRevenueAsync(DateTime date)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/payment/daily-revenue?Date={date:yyyy-MM-dd}");

            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<DailyRevenue>>(JsonOptions);
            if (body?.Succeeded == true)
                return (body.Data, null);

            return (null, FormatApiErrors(body) ?? "No se pudo cargar el resumen del día.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(PaymentListItem? Payment, string? Error)> GetPaymentAsync(int paymentId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/payment/{paymentId}");

            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentListItem>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return (null, FormatApiErrors(body) ?? "No se pudo cargar el detalle del pago.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<InvoiceLineItem> Lines, string? Error)> GetInvoiceLinesAsync(int paymentId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/dental/invoices/payment/{paymentId}");

            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<InvoiceLineItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return ([], FormatApiErrors(body) ?? "No se pudieron cargar las líneas de factura.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdatePaymentAsync(int paymentId, UpdatePaymentRequest request)
    {
        try
        {
            request.PaymentId = paymentId;
            using var response = await Client.PutAsJsonAsync($"/api/payment/{paymentId}", request, JsonOptions);

            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            if (!response.IsSuccessStatusCode && response.Content.Headers.ContentLength == 0)
                return (false, $"No se pudo actualizar el pago ({(int)response.StatusCode}).");

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentUpdateResult>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body) ?? "No se pudo actualizar el pago.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<InvoiceLineItem> Lines, string? Error)> AddInvoiceLineAsync(int paymentId, InvoiceLineInputItem line)
    {
        try
        {
            var request = new AddInvoiceLinesRequest
            {
                PaymentId = paymentId,
                Lines = [line]
            };

            using var response = await Client.PostAsJsonAsync("/api/dental/invoices/lines", request, JsonOptions);

            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<InvoiceLineItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);

            return ([], FormatApiErrors(body) ?? "No se pudo agregar la línea de factura.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteInvoiceLineAsync(int lineId)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/dental/invoices/lines/{lineId}");

            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<InvoiceLineItem>>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(body) ?? "No se pudo eliminar la línea.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public Task<(bool Success, string? Error)> CollectPaymentAsync(int paymentId, CollectPaymentRequest request) =>
        SendPaymentTransitionAsync($"/api/payment/{paymentId}/collect", request);

    public Task<(bool Success, string? Error)> RefundPaymentAsync(int paymentId, string? reason) =>
        SendPaymentTransitionAsync($"/api/payment/{paymentId}/refund", new PaymentReasonRequest { Reason = reason });

    public Task<(bool Success, string? Error)> CancelPaymentAsync(int paymentId, string? reason) =>
        SendPaymentTransitionAsync($"/api/payment/{paymentId}/cancel", new PaymentReasonRequest { Reason = reason });

    private async Task<(bool Success, string? Error)> SendPaymentTransitionAsync<T>(string url, T body)
    {
        try
        {
            using var response = await Client.PutAsJsonAsync(url, body, JsonOptions);

            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            if (!response.IsSuccessStatusCode && response.Content.Headers.ContentLength == 0)
                return (false, $"No se pudo completar la operación ({(int)response.StatusCode}).");

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PaymentListItem>>(JsonOptions);
            if (payload?.Succeeded == true)
                return (true, null);

            return (false, FormatApiErrors(payload) ?? "No se pudo completar la operación.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    private static string BuildPaymentsQuery(BillingPaymentFilters filters)
    {
        var query = new List<string>
        {
            $"PageNumber={filters.PageNumber}",
            $"PageSize={filters.PageSize}"
        };

        if (filters.FromDate.HasValue)
            query.Add($"FromDate={filters.FromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        if (filters.ToDate.HasValue)
            query.Add($"ToDate={filters.ToDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        if (!string.IsNullOrWhiteSpace(filters.Method))
            query.Add($"Method={Uri.EscapeDataString(filters.Method)}");
        if (!string.IsNullOrWhiteSpace(filters.Status))
            query.Add($"Status={Uri.EscapeDataString(filters.Status)}");
        if (!string.IsNullOrWhiteSpace(filters.Search))
            query.Add($"Search={Uri.EscapeDataString(filters.Search.Trim())}");
        if (filters.PatientId is > 0)
            query.Add($"PatientId={filters.PatientId.Value}");

        return string.Join("&", query);
    }

    private string? FormatConnectionError(Exception ex) =>
        ApiConnectionMessages.IsConnectionFailure(ex)
            ? ApiConnectionMessages.ApiUnavailable(_apiSettings.ApiBaseUrl)
            : null;

    private static string? FormatApiErrors<T>(ApiResponse<T>? body) =>
        body?.Errors?.Count > 0
            ? string.Join(" · ", body.Errors)
            : body?.Message;
}
