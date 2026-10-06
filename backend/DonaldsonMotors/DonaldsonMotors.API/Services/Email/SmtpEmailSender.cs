using DonaldsonMotors.API.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace DonaldsonMotors.API.Services.Email
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
    }

    /// <summary>Sends one email over SMTP with MailKit. Throws on failure so the caller can retry.</summary>
    public sealed class SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger) : IEmailSender
    {
        private readonly EmailSettings _settings = settings.Value;

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            email.To.Add(MailboxAddress.Parse(message.ToEmail));
            email.Subject = message.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = message.HtmlBody };

            var security = _settings.SmtpPort == 465
                ? SecureSocketOptions.SslOnConnect
                : _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, security, cancellationToken);
            if (!string.IsNullOrEmpty(_settings.SmtpUser) && !string.IsNullOrEmpty(_settings.SmtpPass))
            {
                await smtp.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass, cancellationToken);
            }
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
            logger.LogInformation("Email sent to {ToEmail} with subject '{Subject}'", message.ToEmail, message.Subject);
        }
    }
}
