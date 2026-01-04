namespace TechHaven.Shared.DTOs.Users;

public class UserDto
{
    public int UserId { get; set; }

    public string UserFullName { get; set; } = string.Empty;
    
    public string? Email { get; set; }

    public string UserName { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}