namespace TechHaven.Shared.DTOs.Auth;

public class OtpVerifyRequestDto
{
    public string OtpSessionId { get; set; } = string.Empty;

    public string OtpCode { get; set; } = string.Empty;

    public string EncryptedDbConfig { get; set; } = string.Empty;
}