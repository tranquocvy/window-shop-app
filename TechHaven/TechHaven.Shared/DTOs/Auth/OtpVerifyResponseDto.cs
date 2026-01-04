namespace TechHaven.Shared.DTOs.Auth;

public class OtpVerifyResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string UserFullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public bool HasSeenGuide { get; set; } = false;

    public string? EncryptedDbConfig { get; set; }
}