namespace TechHaven.Shared.DTOs.Auth;

public class LoginExternalRequestDto
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DbHost { get; set; } = string.Empty;

    public string DbPort { get; set; } = "5432";

    public string DbName { get; set; } = "postgres";

    public string DbUser { get; set; } = string.Empty;

    public string DbPass { get; set; } = string.Empty;
}
