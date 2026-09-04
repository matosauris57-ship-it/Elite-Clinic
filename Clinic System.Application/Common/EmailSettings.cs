namespace Clinic_System.Application.Common
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string FromEmail { get; set; } = string.Empty;
        public string SmtpUser { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;

        public string AuthUser =>
            string.IsNullOrWhiteSpace(SmtpUser) ? FromEmail : SmtpUser;
    }
}
