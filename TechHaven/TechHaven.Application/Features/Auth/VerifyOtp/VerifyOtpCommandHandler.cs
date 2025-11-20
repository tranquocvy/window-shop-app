using TechHaven.Application.Interfaces;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.VerifyOtp;

public class VerifyOtpCommandHandler : ICommandHandler<VerifyOtpCommand, OtpVerifyResponseDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IOtpService _otpService;
  private readonly IJwtTokenService _jwtTokenService;

  public VerifyOtpCommandHandler(
    IUnitOfWork unitOfWork,
    IOtpService otpService,
    IJwtTokenService jwtTokenService)
  {
    _unitOfWork = unitOfWork;
    _otpService = otpService;
    _jwtTokenService = jwtTokenService;
  }

  public async Task<OtpVerifyResponseDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
  {
    // 1. Verify OTP
    var userId = _otpService.ValidateOtp(request.OtpSessionId, request.OtpCode);

    if (!userId.HasValue)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("OtpCode", "Invalid or expired OTP code")
      });
    }

    // 2. Get user with role
    var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken)
        ?? throw new NotFoundException("User", userId.Value);

    // Ensure Role is loaded
    if (user.Role == null)
    {
      var userWithRole = (await _unitOfWork.Users.GetWithRoleAsync(cancellationToken))
          .FirstOrDefault(u => u.UserId == userId.Value)
          ?? throw new NotFoundException("User", userId.Value);
      user = userWithRole;
    }

    // 3. Invalidate OTP after successful verification
    _otpService.InvalidateOtp(request.OtpSessionId);
    
    // 4. Generate Access Token and Refresh Token
    var accessToken = _jwtTokenService.GenerateAccessToken(user);
    var refreshToken = _jwtTokenService.GenerateRefreshToken();

    // 5. Save refresh token to database
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
    user.LastLoginAt = DateTime.UtcNow;

    await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // 6. Return response with both tokens
    return new OtpVerifyResponseDto
    {
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      UserName = user.UserName,
      UserFullName = user.UserFullName,
      Email = user?.Email ?? string.Empty,
      RoleId = user!.RoleId,
      RoleName = user.Role?.RoleName ?? string.Empty
    };
  }
}