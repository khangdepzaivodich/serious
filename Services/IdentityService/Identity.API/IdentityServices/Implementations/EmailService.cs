using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using IdentityService.Identity.API.IdentityServices.Interfaces;
namespace IdentityService.Identity.API.IdentityServices.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.From ?? "no-reply@example.com", _settings.FromDisplayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(to));

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            await client.SendMailAsync(message);
        }
    }
}
