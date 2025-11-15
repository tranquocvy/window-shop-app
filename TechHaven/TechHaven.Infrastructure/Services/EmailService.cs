using Microsoft.Extensions.Logging;
using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services;

/// <summary>
/// Mock email service for development. Replace with real SMTP implementation in production.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendOtpEmailAsync(string recipientEmail, string recipientName, string otpCode, CancellationToken cancellationToken = default)
    {
        // Mock implementation - just log to console
        _logger.LogInformation(
            "Sending OTP Email\n" +
            "To: {Email}\n" +
            "User: {UserName}\n" +
            "OTP Code: {OtpCode}\n" +
            "Valid for: 5 minutes",
            recipientEmail, recipientName, otpCode);

        // TODO: Replace with real SMTP implementation
        // Example using MailKit/MimeKit:
        // var message = new MimeMessage();
        // message.To.Add(new MailboxAddress(recipientName, recipientEmail));
        // message.Subject = "Your TechHaven OTP Code";
        // message.Body = new TextPart("html") { Text = $"Your OTP: {otpCode}" };
        // await _smtpClient.SendAsync(message);

        return Task.CompletedTask;
    }
}