using System.Text;
using Clinic_System.Application.Common;
using Clinic_System.Application.DTOs.Inventory;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Clinic_System.Infrastructure.Services.Email
{
    public class LowStockEmailAlertDispatchService : ILowStockEmailAlertDispatchService
    {
        private readonly IInventoryService _inventory;
        private readonly IEmailService _emailService;
        private readonly IEmailSettingsProvider _emailSettings;
        private readonly ILowStockEmailAlertSettingsService _alertSettings;
        private readonly ILogger<LowStockEmailAlertDispatchService> _logger;

        public LowStockEmailAlertDispatchService(
            IInventoryService inventory,
            IEmailService emailService,
            IEmailSettingsProvider emailSettings,
            ILowStockEmailAlertSettingsService alertSettings,
            ILogger<LowStockEmailAlertDispatchService> logger)
        {
            _inventory = inventory;
            _emailService = emailService;
            _emailSettings = emailSettings;
            _alertSettings = alertSettings;
            _logger = logger;
        }

        [DisableConcurrentExecution(600)]
        public async Task DispatchDueAsync()
        {
            await DispatchInternalAsync(force: false);
        }

        public Task<(bool Sent, int RecipientCount, int LowStockCount, string? Message)> DispatchNowAsync()
            => DispatchInternalAsync(force: true);

        private async Task<(bool Sent, int RecipientCount, int LowStockCount, string? Message)> DispatchInternalAsync(bool force)
        {
            if (!_emailSettings.IsConfigured())
            {
                _logger.LogInformation("Omitiendo alerta de inventario bajo: SMTP no configurado.");
                return (false, 0, 0, "Configure el SMTP en Configuración → Datos de la clínica.");
            }

            var settings = _alertSettings.Get();
            var now = DateTime.Now;

            if (!force)
            {
                if (!settings.ShouldSend(now))
                {
                    if (!settings.Enabled)
                        return (false, 0, 0, "Las alertas de inventario bajo están desactivadas.");
                    if (settings.Recipients.Count == 0)
                        return (false, 0, 0, "No hay correos destinatarios configurados.");
                    if (settings.LastSentDate == DateOnly.FromDateTime(now.Date))
                        return (false, 0, 0, "Ya se envió la alerta de inventario bajo hoy.");
                    return (false, 0, 0, "Aún no es la hora configurada para el envío.");
                }
            }
            else if (!settings.Enabled)
            {
                return (false, 0, 0, "Active las alertas antes de enviar una prueba.");
            }

            if (settings.Recipients.Count == 0)
                return (false, 0, 0, "No hay correos destinatarios configurados.");

            var lowStock = await _inventory.GetLowStockAsync();
            if (lowStock.Count == 0)
            {
                _logger.LogInformation("Sin ítems en stock bajo; no se envía alerta.");
                return (false, 0, 0, force
                    ? "No hay ítems con stock bajo en este momento."
                    : null);
            }

            var clinic = string.IsNullOrWhiteSpace(_emailSettings.Get().SenderName)
                ? "la clínica"
                : _emailSettings.Get().SenderName!;

            var subject = LowStockEmailAlertSettings.Apply(settings.Subject, clinic, lowStock.Count);
            var body = BuildBody(settings, clinic, lowStock);

            var sent = 0;
            foreach (var to in settings.Recipients)
            {
                try
                {
                    await _emailService.SendEmailAsync(to, subject, body);
                    sent++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar alerta de inventario bajo a {Email}", to);
                }
            }

            if (sent == 0)
                return (false, 0, lowStock.Count, "No se pudo enviar el correo. Revise el SMTP.");

            settings.LastSentDate = DateOnly.FromDateTime(now.Date);
            await _alertSettings.SaveAsync(settings);

            _logger.LogInformation(
                "Alerta de inventario bajo enviada a {Recipients} destinatario(s) ({Count} ítems).",
                sent, lowStock.Count);

            return (true, sent, lowStock.Count,
                $"Alerta enviada a {sent} destinatario(s) ({lowStock.Count} ítem(s) en stock bajo).");
        }

        private static string BuildBody(
            LowStockEmailAlertSettings settings,
            string clinic,
            LowStockAlertDTO lowStock)
        {
            var sb = new StringBuilder();
            sb.AppendLine(LowStockEmailAlertSettings.Apply(settings.IntroBody, clinic, lowStock.Count));
            sb.AppendLine();
            sb.AppendLine($"Ítems en stock bajo: {lowStock.Count}");
            sb.AppendLine();

            foreach (var item in lowStock.Items.OrderBy(i => i.Name))
            {
                sb.AppendLine(
                    $"- {item.Name} ({item.Sku}): {item.QuantityOnHand:0.###} {item.Unit} (mínimo {item.MinimumStock:0.###} {item.Unit})");
            }

            sb.AppendLine();
            sb.AppendLine("Este aviso se genera automáticamente desde el módulo de inventario.");
            return sb.ToString();
        }
    }
}
