using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Clinic_System.Core.Messaging;
using MimeKit.Utils;

namespace Clinic_System.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IEmailSettingsProvider _emailSettings;

        public EmailService(IEmailSettingsProvider emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var settings = _emailSettings.Get();
            if (!_emailSettings.IsConfigured())
                throw new InvalidOperationException("Configure el SMTP de la clínica en Configuración → Datos de la clínica.");

            if (!Clinic_System.Core.Validation.ContactEmail.TryValidate(to, out var destination, out var error) || string.IsNullOrWhiteSpace(destination))
                throw new InvalidOperationException(error ?? "El destinatario del correo no es válido.");

            var fromEmail = settings.FromEmail.Trim();
            var domain = fromEmail.Contains('@') ? fromEmail[(fromEmail.IndexOf('@') + 1)..] : "localhost";
            var senderName = string.IsNullOrWhiteSpace(settings.SenderName) ? fromEmail : settings.SenderName.Trim();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, fromEmail));
            message.ReplyTo.Add(new MailboxAddress(senderName, fromEmail));
            message.To.Add(MailboxAddress.Parse(destination));
            message.Subject = subject.Trim();
            message.Date = DateTimeOffset.Now;
            message.MessageId = MimeUtils.GenerateMessageId(domain);
            message.Headers.Replace(HeaderId.Importance, "normal");

            var bodybuilder = new BodyBuilder();
            string html;
            if (MessageBodyFormatting.LooksLikeHtml(body))
            {
                html = MessageBodyFormatting.Sanitize(body);
                bodybuilder.TextBody = MessageBodyFormatting.StripToPlainText(html);
            }
            else
            {
                var plain = body.Trim();
                bodybuilder.TextBody = plain;
                html = "<p>" + WebUtility.HtmlEncode(plain).Replace("\r\n", "\n").Replace("\n", "<br>\n") + "</p>";
            }

            bodybuilder.HtmlBody = WrapHtmlDocument(html);
            message.Body = bodybuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(settings.AuthUser, settings.Password);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        private static string WrapHtmlDocument(string html)
        {
            if (Regex.IsMatch(html, @"<html[\s>]", RegexOptions.IgnoreCase))
                return html;

            var builder = new StringBuilder();
            builder.Append("<!DOCTYPE html><html><body style=\"font-family:Segoe UI,Arial,sans-serif;font-size:15px;line-height:1.5;color:#222\">");
            builder.Append(html);
            builder.Append("</body></html>");
            return builder.ToString();
        }
    }
}
