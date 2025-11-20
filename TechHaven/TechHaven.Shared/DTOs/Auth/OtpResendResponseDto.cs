namespace TechHaven.Shared.DTOs.Auth;

public class OtpResendResponseDto
{
  public bool IsOtpResent { get; set; }
  public string NewOtpSessionId { get; set; } = string.Empty;
}