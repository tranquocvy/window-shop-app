using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMapper;

namespace TechHaven.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, Result<UserInfoDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IMapper _mapper;

  public GetCurrentUserQueryHandler(
      IUnitOfWork unitOfWork,
      IHttpContextAccessor httpContextAccessor,
      IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _httpContextAccessor = httpContextAccessor;
    _mapper = mapper;
  }

  public async Task<Result<UserInfoDto>> Handle(
      GetCurrentUserQuery request,
      CancellationToken cancellationToken)
  {
    try
    {
      // Lấy user ID từ claims trong access token
      var userIdClaim = _httpContextAccessor.HttpContext?.User
          .FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
      {
        return Result<UserInfoDto>.Failure(
            "Unauthorized. Invalid or missing access token.",
            ErrorType.Unauthorized);
      }

      // Lấy thông tin user từ database
      var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

      if (user == null)
      {
        return Result<UserInfoDto>.Failure(
            "User not found.",
            ErrorType.NotFound);
      }

      // Lấy thông tin role
      var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId, cancellationToken);

      if (role == null)
      {
        return Result<UserInfoDto>.Failure(
            "Role not found.",
            ErrorType.NotFound);
      }

      // Map sang DTO
      var userInfo = new UserInfoDto
      {
        UserName = user.UserName ?? string.Empty,
        UserFullName = user.UserFullName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        RoleId = user.RoleId,
        RoleName = role.RoleName ?? string.Empty
      };

      return Result<UserInfoDto>.Success(userInfo);
    }
    catch (Exception ex)
    {
      return Result<UserInfoDto>.Failure(
          $"Failed to get current user: {ex.Message}",
          ErrorType.InternalError);
    }
  }
}