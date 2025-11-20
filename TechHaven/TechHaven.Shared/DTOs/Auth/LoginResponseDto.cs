namespace TechHaven.Shared.DTOs.Auth;

public class LoginResponseDto
{
    public string UserFullName { get; set; } = string.Empty;
    public string MaskedEmail { get; set; } = string.Empty;
    
    // For MFA/OTP flow
    public bool RequiresOtp { get; set; } = false;
    public int OtpExpiresIn { get; set; }

    public string OtpSessionId { get; set; } = string.Empty;
}