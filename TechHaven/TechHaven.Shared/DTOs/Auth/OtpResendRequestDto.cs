namespace TechHaven.Shared.DTOs.Auth;

public class OtpResendRequestDto
{
    public string OtpSessionId { get; set; } = string.Empty;

    public string EncryptedDbConfig { get; set; } = string.Empty;
}