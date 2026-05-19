namespace ReactApp1.Server.Services.Email;

public interface IEmailSenderService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null);
}