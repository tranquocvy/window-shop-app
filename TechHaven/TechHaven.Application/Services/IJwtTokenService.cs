using TechHaven.Domain.Entities;

namespace TechHaven.Application.Services;

/// <summary>
/// Service for generating JWT authentication tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <returns>The JWT token string.</returns>
    string GenerateAccessToken(User user);
}