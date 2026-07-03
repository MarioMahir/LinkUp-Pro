using LinkUpPro.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LinkUpPro.Shared.Services
{

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(
    string email,
    string subject,
    string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(
                        "LinkUpPro",
                        _settings.Email));

                message.To.Add(
                    MailboxAddress.Parse(email));

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlMessage
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient
                {
                    CheckCertificateRevocation = false
                };

                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    false);

                await client.AuthenticateAsync(
                    _settings.Email,
                    _settings.Password);

                await client.SendAsync(message);

                await client.DisconnectAsync(true);

                Console.WriteLine("Correo enviado correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                throw;
            }
        }
    }
}
