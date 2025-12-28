namespace TechHaven.Shared.DTOs.Auth;

/// <summary>
/// DTO for user information response
/// </summary>
public class UserInfoDto
{
    public string UserName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}