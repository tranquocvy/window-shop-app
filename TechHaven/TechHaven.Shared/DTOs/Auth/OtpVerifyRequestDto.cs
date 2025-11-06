namespace TechHaven.Shared.DTOs.Auth;

public class OtpVerifyRequestDto
{
    public int UserId { get; set; }

    public string OtpCode { get; set; } = string.Empty;
}