namespace Clinic_System.Application.Service.Interface
{
    public interface ILowStockEmailAlertSettingsService
    {
        LowStockEmailAlertSettings Get();
        Task SaveAsync(LowStockEmailAlertSettings settings, CancellationToken cancellationToken = default);
    }

    public interface ILowStockEmailAlertDispatchService
    {
        Task DispatchDueAsync();
        Task<(bool Sent, int RecipientCount, int LowStockCount, string? Message)> DispatchNowAsync();
    }
}
