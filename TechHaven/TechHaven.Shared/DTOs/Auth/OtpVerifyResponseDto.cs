namespace TechHaven.Shared.DTOs.Auth;

public class OtpVerifyResponseDto
{
  public string AccessToken { get; set; } = string.Empty;
  public int UserId { get; set; }
  public string UserName { get; set; } = string.Empty;
  public string UserFullName { get; set; } = string.Empty;
  public string RoleName { get; set; } = string.Empty;
}