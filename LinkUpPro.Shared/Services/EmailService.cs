using LinkUpPro.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LinkUpPro.Shared.Services
{
    /// <summary>
    /// Servicio de correo de la capa Shared. Con SMTP configurado envía por MailKit;
    /// sin configurar, guarda cada correo como archivo HTML para poder probar el
    /// registro, la activación y el restablecimiento de contraseña en desarrollo.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (!_settings.IsSmtpConfigured)
            {
                await SaveToFileAsync(email, subject, htmlMessage);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.Email));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

            try
            {
                using var client = new SmtpClient();

                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.Email, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Correo enviado a {Email}: {Subject}", email, subject);
            }
            catch (Exception ex)
            {
                // Sin detalles sensibles hacia el usuario: el controlador muestra un mensaje general.
                _logger.LogError(ex, "Error enviando correo a {Email}", email);
                throw new InvalidOperationException("No fue posible enviar el correo electrónico.");
            }
        }

        private async Task SaveToFileAsync(string email, string subject, string htmlMessage)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), _settings.OutputFolder);
            Directory.CreateDirectory(folder);

            var safeEmail = string.Concat(email.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
            var path = Path.Combine(folder, $"{DateTime.Now:yyyyMMdd-HHmmss-fff}_{safeEmail}.html");

            var html = $"""
                <!DOCTYPE html>
                <html lang="es"><head><meta charset="utf-8"><title>{subject}</title></head>
                <body style="font-family:Segoe UI,Arial,sans-serif;max-width:640px;margin:24px auto;padding:0 16px">
                <div style="background:#f1f3f5;border-radius:8px;padding:12px 16px;font-size:13px;color:#495057">
                  <strong>Correo simulado (sin SMTP configurado)</strong><br>
                  Para: {email}<br>
                  Asunto: {subject}<br>
                  Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
                </div>
                <hr>
                {htmlMessage}
                </body></html>
                """;

            await File.WriteAllTextAsync(path, html);

            _logger.LogWarning("SMTP no configurado. Correo para {Email} guardado en {Path}", email, path);
        }
    }
}
