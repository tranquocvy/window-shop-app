namespace TechHaven.Application.Interfaces;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an OTP code via email.
    /// </summary>
    /// <param name="recipientEmail">The recipient's email address.</param>
    /// <param name="recipientName">The recipient's name.</param>
    /// <param name="otpCode">The OTP code to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendOtpEmailAsync(
        string recipientEmail, 
        string recipientName, 
        string otpCode, 
        CancellationToken cancellationToken = default);
}