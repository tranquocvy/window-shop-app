using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services.Email;

/// <summary>
/// Email service using Brevo (Sendinblue) for fast and reliable email delivery.
/// Free tier: 300 emails/day
/// </summary>
public class BrevoEmailService : IEmailService
{
    private readonly ILogger<BrevoEmailService> _logger;
    private readonly TechHaven.Infrastructure.Configuration.BrevoSettings _settings;
    private readonly sib_api_v3_sdk.Api.TransactionalEmailsApi _apiInstance;

    public BrevoEmailService(
        ILogger<BrevoEmailService> logger,
        IOptions<TechHaven.Infrastructure.Configuration.BrevoSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Brevo API Key is not configured");
        }

        // Configure API client
        // sib_api_v3_sdk.Client.Configuration.Default.ApiKey.Add("api-key", _settings.ApiKey);
        if (sib_api_v3_sdk.Client.Configuration.Default.ApiKey.ContainsKey("api-key"))
        {
            sib_api_v3_sdk.Client.Configuration.Default.ApiKey["api-key"] = _settings.ApiKey;
        }
        else
        {
            sib_api_v3_sdk.Client.Configuration.Default.ApiKey.Add("api-key", _settings.ApiKey);
        }
        _apiInstance = new sib_api_v3_sdk.Api.TransactionalEmailsApi();
    }

    public async Task SendOtpEmailAsync(
        string recipientEmail,
        string recipientName,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Attempting to send OTP email via Brevo to {Email} (User: {UserName})",
            MaskEmail(recipientEmail), recipientName);

        try
        {
            // Create sender
            var sender = new sib_api_v3_sdk.Model.SendSmtpEmailSender(
                name: _settings.SenderName,
                email: _settings.SenderEmail
            );

            // Create recipient
            var to = new System.Collections.Generic.List<sib_api_v3_sdk.Model.SendSmtpEmailTo>
            {
                new sib_api_v3_sdk.Model.SendSmtpEmailTo(
                    email: recipientEmail,
                    name: recipientName
                )
            };

            // Create email content
            var subject = "Your TechHaven OTP Code";
            var htmlContent = GenerateOtpEmailHtml(recipientName, otpCode);
            var textContent = $"Your OTP code is: {otpCode}. Valid for 5 minutes.";

            // Create email message
            var sendSmtpEmail = new sib_api_v3_sdk.Model.SendSmtpEmail(
                sender: sender,
                to: to,
                subject: subject,
                htmlContent: htmlContent,
                textContent: textContent
            );

            // Send email
            var result = await _apiInstance.SendTransacEmailAsync(sendSmtpEmail);

            _logger.LogInformation(
                "OTP email sent successfully via Brevo to {Email}. MessageId: {MessageId}",
                MaskEmail(recipientEmail),
                result.MessageId);
        }
        catch (sib_api_v3_sdk.Client.ApiException ex)
        {
            var errorCode = ex.ErrorCode;
            var errorMessage = ex.Message;
            var errorContent = ex.ErrorContent?.ToString() ?? "No content";

            //   _logger.LogError(
            //       ex,
            //       "Brevo API error. StatusCode: {StatusCode}, Message: {Message}, Response: {Response}",
            //       errorCode,
            //       errorMessage,
            //       errorContent);

            throw new InvalidOperationException(
                $"Brevo API error ({errorCode}): {errorMessage}", ex);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send OTP email via Brevo to {Email}",
                MaskEmail(recipientEmail));

            throw new InvalidOperationException(
                $"Failed to send OTP email: {ex.Message}", ex);
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
                    <span style='color: #000; font-size: 32px; font-weight: 600; letter-spacing: 8px; font-family: ""Courier New"", monospace;'>{otpCode}</span>
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
                © 2025 TechHaven - Powered by Brevo
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