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
    try
    {
      // 1. Create email message
      var message = new MimeMessage();
      message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
      message.To.Add(new MailboxAddress(recipientName, recipientEmail));
      message.Subject = "Your TechHaven OTP Code";

      // 2. Create HTML body
      var bodyBuilder = new BodyBuilder
      {
        HtmlBody = GenerateOtpEmailHtml(recipientName, otpCode)
      };
      message.Body = bodyBuilder.ToMessageBody();

      // 3. Send email using SMTP
      using var client = new SmtpClient();

      // Connect to SMTP server
      await client.ConnectAsync(
          _smtpSettings.Host,
          _smtpSettings.Port,
          _smtpSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
          cancellationToken);

      // Authenticate
      await client.AuthenticateAsync(
          _smtpSettings.Username,
          _smtpSettings.Password,
          cancellationToken);

      // Send message
      await client.SendAsync(message, cancellationToken);

      // Disconnect
      await client.DisconnectAsync(true, cancellationToken);

      _logger.LogInformation(
          "OTP email sent successfully to {Email} (User: {UserName})",
          recipientEmail, recipientName);
    }
    catch (Exception ex)
    {
      _logger.LogError(
          ex,
          "Failed to send OTP email to {Email}. Error: {ErrorMessage}",
          recipientEmail, ex.Message);

      // Re-throw để caller xử lý
      throw new InvalidOperationException($"Failed to send OTP email: {ex.Message}", ex);
    }
  }

  private string GenerateOtpEmailHtml(string recipientName, string otpCode)
  {
    return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>TechHaven OTP Code</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>TechHaven</h1>
    </div>
    
    <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.1);'>
        <h2 style='color: #667eea; margin-top: 0;'>Xin chào {recipientName},</h2>
        
        <p>Chúng tôi đã nhận được yêu cầu đăng nhập vào tài khoản TechHaven của bạn.</p>
        
        <p>Mã OTP của bạn là:</p>
        
        <div style='background-color: #667eea; color: white; font-size: 32px; font-weight: bold; text-align: center; padding: 20px; border-radius: 5px; letter-spacing: 8px; margin: 20px 0;'>
            {otpCode}
        </div>
        
        <p style='color: #e74c3c; font-weight: bold;'>⚠️ Mã này sẽ hết hạn sau 5 phút.</p>
        
        <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
        
        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
        
        <p style='font-size: 12px; color: #888; text-align: center;'>
            Email này được gửi tự động, vui lòng không trả lời.<br>
            © 2025 TechHaven. All rights reserved.
        </p>
    </div>
</body>
</html>";
  }
}