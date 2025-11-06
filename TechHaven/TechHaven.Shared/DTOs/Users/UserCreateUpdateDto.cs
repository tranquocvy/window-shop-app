namespace TechHaven.Shared.DTOs.Users;

public class UserCreateUpdateDto
{
    public string UserFullName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}


