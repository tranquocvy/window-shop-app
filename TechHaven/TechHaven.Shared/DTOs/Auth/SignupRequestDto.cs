namespace TechHaven.Shared.DTOs.Auth;

public class SignupRequestDto
{
  public string UserFullName { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public string UserName { get; set; } = string.Empty;

  public string Password { get; set; } = string.Empty;

  public int RoleId { get; set; } = 1;

  // public string ConfirmPassword { get; set; } = string.Empty;
}