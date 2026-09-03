namespace Clinic_System.Application.Common
{
    public class ClinicEmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string FromEmail { get; set; } = string.Empty;
        public string SmtpUser { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool PasswordConfigured { get; set; }
        public bool IsConfigured { get; set; }
    }

    public class SendPatientEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
