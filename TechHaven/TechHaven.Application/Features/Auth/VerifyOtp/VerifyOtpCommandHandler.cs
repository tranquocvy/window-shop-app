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
    if (!_otpService.ValidateOtp(request.UserId, request.OtpCode))
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("OtpCode", "Invalid or expired OTP code")
      });
    }

    // 2. Get user with role
    var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
        ?? throw new NotFoundException("User", request.UserId);

    // Ensure Role is loaded
    if (user.Role == null)
    {
      var userWithRole = (await _unitOfWork.Users.GetWithRoleAsync(cancellationToken))
          .FirstOrDefault(u => u.UserId == request.UserId)
          ?? throw new NotFoundException("User", request.UserId);
      user = userWithRole;
    }

    // 3. Invalidate OTP after successful verification
    _otpService.InvalidateOtp(request.UserId);

    // 4. Generate JWT token
    var token = _jwtTokenService.GenerateAccessToken(user);

    // 5. Return response
    return new OtpVerifyResponseDto
    {
      AccessToken = token,
      UserId = user.UserId,
      UserName = user.UserName,
      UserFullName = user.UserFullName,
      RoleName = user.Role?.RoleName ?? string.Empty
    };
  }
}