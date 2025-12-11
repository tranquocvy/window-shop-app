using TechHaven.Application.Interfaces;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtTokenService _jwtTokenService;

  public RefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService)
  {
    _unitOfWork = unitOfWork;
    _jwtTokenService = jwtTokenService;
  }

  public async Task<RefreshTokenResponseDto> Handle(
    RefreshTokenCommand request,
    CancellationToken cancellationToken)
  {
    // 1. Validate refresh token format
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("RefreshToken", "Refresh token is required")
      });
    }

    // 2. Find user by refresh token
    var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
    var user = users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

    if (user == null)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("RefreshToken", "Invalid refresh token")
      });
    }

    // 3. Check if refresh token is expired
    if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("RefreshToken", "Refresh token has expired")
      });
    }

        // 4. Check if user is still active - thêm: check Date.UtcNow - user.createdAt >= 15 days
     if (!user.IsActive && (DateTime.UtcNow - user.CreatedAt).TotalDays >= 15)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("User", "User account is deactivated after 15 days of trial mode")
      });
    }

    // 5. Ensure Role is loaded
    if (user.Role == null)
    {
      var userWithRole = (await _unitOfWork.Users.GetWithRoleAsync(cancellationToken))
          .FirstOrDefault(u => u.UserId == user.UserId)
          ?? throw new NotFoundException("User", user.UserId);
      user = userWithRole;
    }

    // 6. Generate new access token and refresh token
    var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
    var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

    // 7. Update user with new refresh token
    user.RefreshToken = newRefreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

    await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // 8. Return new tokens
    return new RefreshTokenResponseDto
    {
      AccessToken = newAccessToken,
      NewRefreshToken = newRefreshToken
    };
  }
}