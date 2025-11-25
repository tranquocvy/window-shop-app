using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services;

/// <summary>
/// Password hashing service using BCrypt algorithm.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12; // Config được từ appsettings
    
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            // Timing attack protection
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            // KHÔNG return false ngay - timing attack
            _ = BCrypt.Net.BCrypt.HashPassword(password); // dummy hash
            return false;
        }
    }
}