using System.Net.Http.Json;
using System.Text.Json;
using DentalCare.Admin.Models;
using Microsoft.Extensions.Options;

namespace DentalCare.Admin.Services;

public class InventoryMaintenanceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AuthenticatedApiClient _apiClient;
    private readonly TokenStorage _tokenStorage;
    private readonly ApiSettings _apiSettings;

    public InventoryMaintenanceService(
        AuthenticatedApiClient apiClient,
        TokenStorage tokenStorage,
        IOptions<ApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
        _apiSettings = apiSettings.Value;
    }

    private HttpClient Client => _apiClient.Client;

    public async Task<(List<InventoryItemListItem> Items, string? Error)> GetItemsAsync(
        bool activeOnly = false, bool lowStockOnly = false)
    {
        try
        {
            using var response = await Client.GetAsync(
                $"/api/inventory/items?activeOnly={activeOnly.ToString().ToLowerInvariant()}&lowStockOnly={lowStockOnly.ToString().ToLowerInvariant()}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<InventoryItemListItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return ([], body?.Message ?? "No se pudo cargar el inventario.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error, int? Id)> CreateAsync(CreateInventoryItemRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("/api/inventory/items", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response), null);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryItemListItem>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (true, null, body.Data.Id);
            return (false, body?.Message ?? "No se pudo crear el ítem.", null);
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message, null);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateInventoryItemRequest request)
    {
        try
        {
            request.Id = id;
            var response = await Client.PutAsJsonAsync($"/api/inventory/items/{id}", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryItemListItem>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo actualizar el ítem.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        try
        {
            using var response = await Client.DeleteAsync($"/api/inventory/items/{id}");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo eliminar el ítem.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> RegisterEntryAsync(int id, StockEntryRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"/api/inventory/items/{id}/entries", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<StockMovementListItem>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo registrar la entrada.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> RegisterAdjustmentAsync(int id, StockAdjustmentRequest request)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"/api/inventory/items/{id}/adjustments", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<StockMovementListItem>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo registrar el ajuste.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<StockMovementListItem> Items, string? Error)> GetMovementsAsync(int id, int take = 50)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/inventory/items/{id}/movements?take={take}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ([], ApiConnectionMessages.UnauthorizedSession(_tokenStorage));
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<StockMovementListItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return ([], body?.Message ?? "No se pudieron cargar los movimientos.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(List<ProcedureMaterialItem> Items, string? Error)> GetBomAsync(int procedureId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/inventory/procedures/{procedureId}/bom");
            if (ApiConnectionMessages.IsRateLimited(response))
                return ([], await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProcedureMaterialItem>>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return ([], body?.Message ?? "No se pudo cargar el BOM.");
        }
        catch (Exception ex)
        {
            return ([], FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ReplaceBomAsync(int procedureId, ReplaceProcedureBomRequest request)
    {
        try
        {
            var response = await Client.PutAsJsonAsync($"/api/inventory/procedures/{procedureId}/bom", request, JsonOptions);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (false, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProcedureMaterialItem>>>(JsonOptions);
            if (body?.Succeeded == true)
                return (true, null);
            return (false, body?.Message ?? "No se pudo guardar el BOM.");
        }
        catch (Exception ex)
        {
            return (false, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(MaterialConsumptionProposal? Data, string? Error)> ProposeConsumptionAsync(int treatmentId)
    {
        try
        {
            using var response = await Client.GetAsync($"/api/inventory/consumptions/propose/{treatmentId}");
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<MaterialConsumptionProposal>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return (null, body?.Message ?? "No se pudo proponer el consumo.");
        }
        catch (Exception ex)
        {
            return (null, FormatConnectionError(ex) ?? ex.Message);
        }
    }

    public async Task<(LowStockAlert? Data, string? Error)> GetLowStockAsync()
    {
        try
        {
            using var response = await Client.GetAsync("/api/inventory/low-stock");
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
                return (null, null);
            if (ApiConnectionMessages.IsRateLimited(response))
                return (null, await ApiConnectionMessages.GetRateLimitMessageAsync(response));

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<LowStockAlert>>(JsonOptions);
            if (body?.Succeeded == true && body.Data != null)
                return (body.Data, null);
            return (null, body?.Message);
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
