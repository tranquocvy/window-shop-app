namespace TechHaven.Shared.DTOs.Auth;

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string NewRefreshToken { get; set; } = string.Empty;
}
