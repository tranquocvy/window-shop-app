namespace TechHaven.Shared.DTOs.Auth;

public class LoginResponseDto
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserFullName { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    // For MFA/OTP flow
    public bool RequiresOtp { get; set; } = false;
}