using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using TechHaven.Application.Interfaces;
using TechHaven.Infrastructure.Configuration;

namespace TechHaven.Infrastructure.Services;

/// <summary>
/// Email service using SMTP for sending emails.
/// </summary>
public class EmailService : IEmailService
{
  private readonly ILogger<EmailService> _logger;
  private readonly SmtpSettings _smtpSettings;

  public EmailService(
      ILogger<EmailService> logger,
      IOptions<SmtpSettings> smtpSettings)
  {
    _logger = logger;
    _smtpSettings = smtpSettings.Value;
  }

  public async Task SendOtpEmailAsync(
      string recipientEmail,
      string recipientName,
      string otpCode,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation(
        "Attempting to send OTP email to {Email} (User: {UserName})",
        MaskEmail(recipientEmail), recipientName);

    try
    {
      // 1. Create email message
      var message = new MimeMessage();
      message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
      message.To.Add(new MailboxAddress(recipientName, recipientEmail));
      message.Subject = "Your TechHaven OTP Code";

      _logger.LogDebug("Email message created with subject: {Subject}", message.Subject);

      // 2. Create HTML body
      var bodyBuilder = new BodyBuilder
      {
        HtmlBody = GenerateOtpEmailHtml(recipientName, otpCode)
      };
      message.Body = bodyBuilder.ToMessageBody();

      // 3. Send email using SMTP
      using var client = new SmtpClient();

      _logger.LogDebug(
        "Connecting to SMTP server: {Host}:{Port} (SSL: {EnableSsl})",
        _smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSsl
      );

      // Connect to SMTP server
      await client.ConnectAsync(
          _smtpSettings.Host,
          _smtpSettings.Port,
          _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
          cancellationToken);

      _logger.LogDebug("Connected to SMTP server successfully");

      // Authenticate
      await client.AuthenticateAsync(
          _smtpSettings.Username,
          _smtpSettings.Password,
          cancellationToken);

      _logger.LogDebug("SMTP authentication successful");

      // Send message
      await client.SendAsync(message, cancellationToken);

      await client.DisconnectAsync(true, cancellationToken);

      _logger.LogInformation(
        "OTP email sent successfully to {Email}",
        MaskEmail(recipientEmail)
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to send OTP email to {Email}. SMTP: {SmtpHost}:{SmtpPort}",
          MaskEmail(recipientEmail), _smtpSettings.Host, _smtpSettings.Port);

      // Re-throw để caller xử lý
      throw new InvalidOperationException($"Failed to send OTP email: {ex.Message}", ex);
    }
  }

  private string GenerateOtpEmailHtml(string recipientName, string otpCode)
  {
    _logger.LogDebug("Generating OTP email HTML for {UserName}", recipientName);

    return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>TechHaven OTP Code</title>
</head>
<body style='margin: 0; padding: 0; background: #f5f5f5; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;'>
    <div style='max-width: 500px; margin: 60px auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08);'>
        
        <!-- Header -->
        <div style='padding: 40px 40px 30px; border-bottom: 1px solid #e8e8e8;'>
            <h1 style='color: #000; margin: 0; font-size: 22px; font-weight: 600; letter-spacing: -0.3px;'>TechHaven</h1>
        </div>
        
        <!-- Content -->
        <div style='padding: 40px;'>
            <p style='color: #666; font-size: 15px; line-height: 1.6; margin: 0 0 32px;'>
                Xin chào <strong style='color: #000;'>{recipientName}</strong>,<br>
                Sử dụng mã xác thực bên dưới để đăng nhập.
            </p>
            
            <!-- OTP Box -->
            <div style='text-align: center; margin: 32px 0;'>
                <div style='background: #f8f8f8; border: 1px solid #e0e0e0; border-radius: 6px; padding: 24px; display: inline-block; min-width: 280px;'>
                    <span id='otp-code' style='color: #000; font-size: 32px; font-weight: 600; letter-spacing: 8px; font-family: ""Courier New"", monospace;'>{otpCode}</span>
                </div>
                
                <div style='margin-top: 16px;'>
                    <button onclick='navigator.clipboard.writeText(document.getElementById(""otp-code"").innerText).then(() => {{ this.innerHTML = ""Đã copy""; setTimeout(() => {{ this.innerHTML = ""Copy mã""; }}, 2000); }})' 
                        style='background: #000; color: #fff; border: none; padding: 10px 20px; font-size: 13px; font-weight: 500; border-radius: 4px; cursor: pointer; font-family: inherit;'
                        onmouseover='this.style.background=""#333""'
                        onmouseout='this.style.background=""#000""'>
                        Copy mã
                    </button>
                </div>
            </div>
            
            <p style='color: #999; font-size: 13px; margin: 32px 0 0; line-height: 1.5;'>
                Mã có hiệu lực trong <strong style='color: #666;'>5 phút</strong>.<br>
                Nếu bạn không yêu cầu mã này, hãy bỏ qua email.
            </p>
        </div>
        
        <!-- Footer -->
        <div style='padding: 24px 40px; background: #fafafa; border-top: 1px solid #e8e8e8;'>
            <p style='color: #999; font-size: 11px; margin: 0; text-align: center;'>
                © 2025 TechHaven
            </p>
        </div>
    </div>
</body>
</html>";
  }

  /// <summary>
  /// Mask email for security logging
  /// </summary>
  private string MaskEmail(string email)
  {
    if (string.IsNullOrEmpty(email) || !email.Contains('@'))
      return "***@***.***";

    var parts = email.Split('@');
    var localPart = parts[0];
    var domain = parts[1];

    var maskedLocal = localPart.Length > 2
        ? localPart.Substring(0, 2) + "***"
        : "***";

    return $"{maskedLocal}@{domain}";
  }
}