namespace TechHaven.Shared.DTOs.Auth;

public class SignupResponseDto
{
  public int UserId { get; set; }

  public string UserFullName { get; set; } = string.Empty;

  public string UserName { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public int RoleId { get; set; }

  public string Message { get; set; } = string.Empty;
}