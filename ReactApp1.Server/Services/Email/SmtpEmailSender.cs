using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace ReactApp1.Server.Services.Email;

public sealed class SmtpEmailSender(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSenderService
{
    private readonly SmtpEmailOptions _options = options.Value;
    private readonly ILogger<SmtpEmailSender> _logger = logger;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            _logger.LogWarning("SMTP settings missing: Host or FromEmail not configured.");
            throw new InvalidOperationException("SMTP email settings are not configured.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, string.IsNullOrWhiteSpace(_options.FromName) ? _options.FromEmail : _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail));

        if (!string.IsNullOrWhiteSpace(textBody))
        {
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 10000
        };

        // Ensure explicit credentials usage
        client.UseDefaultCredentials = false;
        client.Credentials = string.IsNullOrWhiteSpace(_options.UserName)
            ? CredentialCache.DefaultNetworkCredentials
            : new NetworkCredential(_options.UserName, _options.Password);

        _logger.LogInformation("Sending email to {Email} via SMTP host {Host}:{Port} (From: {From})", toEmail, _options.Host, _options.Port, _options.FromEmail);

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP send failed for {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while sending email to {Email}: {Message}", toEmail, ex.Message);
            throw;
        }
    }
}