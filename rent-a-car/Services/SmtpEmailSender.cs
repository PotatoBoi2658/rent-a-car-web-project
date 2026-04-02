using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
/// <summary>
/// send email using SMTP with MailKit, configured via SmtpOptions  
/// <summary>
namespace rent_a_car.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // If SMTP is not configured (no credentials or no sender), skip sending silently.
            if (string.IsNullOrWhiteSpace(_options.Host)
                || string.IsNullOrWhiteSpace(_options.SenderEmail)
                || string.IsNullOrWhiteSpace(_options.Username)
                || string.IsNullOrWhiteSpace(_options.Password))
            {
                _logger.LogInformation("SMTP not configured or credentials missing; skipping email to {Recipient}.", email);
                return;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.SenderName ?? _options.SenderEmail, _options.SenderEmail));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

                using var client = new SmtpClient();
                var secure = _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                _logger.LogDebug("Connecting to SMTP {Host}:{Port} SSL={UseSsl}", _options.Host, _options.Port, _options.UseSsl);
                await client.ConnectAsync(_options.Host, _options.Port, secure);

                _logger.LogDebug("Authenticating SMTP as {Username}", _options.Username);
                await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Sent email to {Recipient}", email);
            }
            catch (Exception ex)
            {
                // Log error but do not throw so the app continues to work without SMTP configured.
                _logger.LogError(ex, "Failed to send email to {Recipient}; continuing without email.", email);
            }
        }
    }
}