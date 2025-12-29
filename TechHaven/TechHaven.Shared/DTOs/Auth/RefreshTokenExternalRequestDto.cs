namespace TechHaven.Shared.DTOs.Auth;

public class RefreshTokenExternalRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
    public string EncryptedDbConfig { get; set; } = string.Empty;
}
