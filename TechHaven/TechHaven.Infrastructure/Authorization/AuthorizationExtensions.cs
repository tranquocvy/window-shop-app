using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TechHaven.Infrastructure.Authorization;

/// <summary>
/// Extension methods để lấy thông tin user từ HttpContext
/// </summary>
public static class AuthorizationExtensions
{
  /// <summary>
  /// Lấy UserId từ access token
  /// </summary>
  public static int? GetCurrentUserId(this ClaimsPrincipal user)
  {
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                      ?? user.FindFirst("sub");

    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
    {
      return userId;
    }

    return null;
  }

  /// <summary>
  /// Lấy UserId từ HttpContext (wrapper tiện lợi)
  /// </summary>
  public static int? GetCurrentUserId(this HttpContext httpContext)
  {
    return httpContext.User.GetCurrentUserId();
  }

  /// <summary>
  /// Lấy Role của user
  /// </summary>
  public static string? GetCurrentUserRole(this ClaimsPrincipal user)
  {
    return user.FindFirst(ClaimTypes.Role)?.Value;
  }

  /// <summary>
  /// Kiểm tra user có phải Admin không
  /// </summary>
  public static bool IsAdmin(this ClaimsPrincipal user)
  {
    return user.IsInRole(Roles.Admin);
  }

  /// <summary>
  /// Kiểm tra user có phải Seller không
  /// </summary>
  public static bool IsSeller(this ClaimsPrincipal user)
  {
    return user.IsInRole(Roles.Seller);
  }

  /// <summary>
  /// Lấy UserName từ token
  /// </summary>
  public static string? GetCurrentUserName(this ClaimsPrincipal user)
  {
    return user.FindFirst(ClaimTypes.Name)?.Value;
  }
}