namespace TechHaven.Shared.DTOs.Auth;

public class IsActiveResponseDto
{
    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates the number of days remaining before the account is deactivated.
    /// </summary>
    public int DaysRemain { get; set; }
}