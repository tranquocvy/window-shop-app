using Microsoft.Extensions.Options;
using TechHaven.Application.Interfaces;
using TechHaven.Infrastructure.Configuration; // Thêm dòng này

namespace TechHaven.Infrastructure.Services;

/// <summary>
/// Password hashing service using BCrypt algorithm.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public PasswordHasher(IOptions<SecuritySettings> settings)
    {
        _workFactor = settings.Value.BcryptWorkFactor;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: _workFactor);
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