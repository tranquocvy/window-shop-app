using TechHaven.Domain.Entities;
using System.Security.Claims;

namespace TechHaven.Application.Interfaces;

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

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>A cryptographically secure refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates an expired access token and extracts claims.
    /// </summary>
    /// <param name="token">The expired access token.</param>
    /// <returns>ClaimsPrincipal if valid, null otherwise.</returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}