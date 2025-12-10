using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TechHaven.Infrastructure.Authorization.Handlers;

/// <summary>
/// Requirement để kiểm tra user có quyền truy cập resource hay không
/// </summary>
public class SameUserRequirement : IAuthorizationRequirement
{
  public string UserIdClaimType { get; }

  public SameUserRequirement(string userIdClaimType = ClaimTypes.NameIdentifier)
  {
    UserIdClaimType = userIdClaimType;
  }
}

/// <summary>
/// Handler kiểm tra user chỉ được truy cập resource của chính mình
/// (trừ Admin có thể truy cập tất cả)
/// </summary>
public class SameUserAuthorizationHandler : AuthorizationHandler<SameUserRequirement>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      SameUserRequirement requirement)
  {
    // Lấy userId từ token
    var userIdClaim = context.User.FindFirst(requirement.UserIdClaimType);
    if (userIdClaim == null)
    {
      return Task.CompletedTask; // Fail
    }

    // Admin có thể truy cập tất cả
    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
    if (roleClaim?.Value == Roles.Admin)
    {
      context.Succeed(requirement);
      return Task.CompletedTask;
    }

    // Kiểm tra resource có chứa userId không
    // Resource được pass qua context.Resource
    if (context.Resource is IUserOwnedResource ownedResource)
    {
      if (ownedResource.UserId.ToString() == userIdClaim.Value)
      {
        context.Succeed(requirement);
      }
    }

    return Task.CompletedTask;
  }
}

/// <summary>
/// Interface đánh dấu resource thuộc về một user
/// </summary>
public interface IUserOwnedResource
{
  int? UserId { get; }
}

/// <summary>
/// Requirement kiểm tra quyền trên AppSetting
/// </summary>
public class AppSettingAccessRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Handler kiểm tra quyền truy cập AppSetting
/// Rule: 
/// - Admin: có thể xem/sửa tất cả setting (bao gồm system setting)
/// - Seller: có thể xem system settings và xem/sửa setting của mình
/// - System settings: Tất cả users đều có thể đọc, chỉ Admin mới được sửa
/// </summary>
public class AppSettingAuthorizationHandler
    : AuthorizationHandler<AppSettingAccessRequirement>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      AppSettingAccessRequirement requirement)
  {
    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
      return Task.CompletedTask; // Fail
    }

    var roleClaim = context.User.FindFirst(ClaimTypes.Role);
    var currentUserIdValue = userIdClaim.Value;

    // Admin có full quyền
    if (roleClaim?.Value == Roles.Admin)
    {
      context.Succeed(requirement);
      return Task.CompletedTask;
    }

    // Resource được truyền vào context.Resource
    if (context.Resource is AppSettingAuthorizationContext settingContext)
    {
      // Nếu là system setting (UserId = null) -> Tất cả users đều có thể đọc
      // (Việc kiểm tra quyền sửa sẽ được xử lý ở controller level)
      if (settingContext.TargetUserId == null)
      {
        // Cho phép đọc system settings
        context.Succeed(requirement);
        return Task.CompletedTask;
      }

      // Nếu là user setting -> chỉ được truy cập setting của mình
      var targetUserIdString = settingContext.TargetUserId.Value.ToString();
      if (targetUserIdString == currentUserIdValue)
      {
        context.Succeed(requirement);
      }
    }

    return Task.CompletedTask;
  }
}

/// <summary>
/// Context để pass thông tin setting vào Authorization Handler
/// </summary>
public class AppSettingAuthorizationContext
{
  public int? TargetUserId { get; set; }
  public string Key { get; set; } = string.Empty;
  public bool IsSystemSetting => TargetUserId == null;
}