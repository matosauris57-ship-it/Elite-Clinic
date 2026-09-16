using Clinic_System.Application.Common;

namespace Clinic_System.Application.Service.Interface
{
    public interface ICampaignBookingNotifySettingsService
    {
        CampaignBookingNotifySettings Get();
        Task SaveAsync(CampaignBookingNotifySettings settings, CancellationToken cancellationToken = default);
    }
}
