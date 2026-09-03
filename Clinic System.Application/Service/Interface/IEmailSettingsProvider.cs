namespace Clinic_System.Application.Service.Interface
{
    public interface IEmailSettingsProvider
    {
        EmailSettings Get();
        bool IsConfigured();
        Task SaveAsync(EmailSettings settings, bool keepExistingPassword = false, CancellationToken cancellationToken = default);
    }
}
