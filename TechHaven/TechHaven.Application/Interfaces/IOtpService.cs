namespace TechHaven.Application.Interfaces;

/// <summary>
/// Service for generating and validating One-Time Passwords (OTP).
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a 6-digit OTP code and stores it with expiration time.
    /// </summary>
    /// <param name="userId">The user ID for whom the OTP is generated.</param>
    /// <returns>The generated OTP code.</returns>
    string GenerateOtp(int userId);

    /// <summary>
    /// Validates an OTP code using session ID.
    /// </summary>
    /// <param name="otpSessionId">The OTP session ID to validate.</param>
    /// <param name="otpCode">The OTP code to validate.</param>
    /// <returns>The user ID if valid, null if invalid or expired.</returns>
    int? ValidateOtp(string otpSessionId, string otpCode);

    /// <summary>
    /// Invalidates (removes) the OTP using session ID after successful validation.
    /// </summary>
    /// <param name="otpSessionId">The OTP session ID to invalidate.</param>
    void InvalidateOtp(string otpSessionId);
}