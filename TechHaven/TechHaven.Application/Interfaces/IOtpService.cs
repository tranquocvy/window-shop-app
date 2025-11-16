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
    /// Validates an OTP code for a user.
    /// </summary>
    /// <param name="userId">The user ID to validate the OTP for.</param>
    /// <param name="otpCode">The OTP code to validate.</param>
    /// <returns>True if the OTP is valid and not expired, false otherwise.</returns>
    bool ValidateOtp(int userId, string otpCode);

    /// <summary>
    /// Invalidates (removes) the OTP for a user after successful validation.
    /// </summary>
    /// <param name="userId">The user ID whose OTP should be invalidated.</param>
    void InvalidateOtp(int userId);
}