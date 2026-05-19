using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace ReactApp1.Server.Services.Email;

public sealed class FileEmailSender : IEmailSenderService
{
    private readonly string _outDir;
    private readonly ILogger<FileEmailSender> _logger;

    public FileEmailSender(IWebHostEnvironment env, ILogger<FileEmailSender> logger)
    {
        _logger = logger;
        _outDir = Path.Combine(env.ContentRootPath, "DevEmails");
        try
        {
            Directory.CreateDirectory(_outDir);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DevEmails directory: {Message}", ex.Message);
        }
    }

    public Task SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null)
    {
        var fileName = Path.Combine(_outDir, $"email_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid()}.html");
        var sb = new StringBuilder();
        sb.AppendLine($"To: {toEmail}");
        sb.AppendLine($"Subject: {subject}");
        sb.AppendLine("--- TEXT BODY ---");
        sb.AppendLine(textBody ?? "");
        sb.AppendLine("--- HTML BODY ---");
        sb.AppendLine(htmlBody ?? "");

        try
        {
            File.WriteAllText(fileName, sb.ToString());
            _logger.LogInformation("Saved development email to {File}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write development email file: {Message}", ex.Message);
            throw;
        }

        return Task.CompletedTask;
    }
}
