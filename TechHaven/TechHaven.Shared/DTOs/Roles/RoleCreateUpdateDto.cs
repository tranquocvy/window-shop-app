namespace TechHaven.Shared.DTOs.Roles;

public class RoleCreateUpdateDto
{
    public string RoleName { get; set; } = string.Empty;

    public string? Description { get; set; }
}