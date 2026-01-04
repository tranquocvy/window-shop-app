using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.ResendOtp;

public class ResendOtpCommandHandler : ICommandHandler<ResendOtpCommand, OtpResendResponseDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IOtpService _otpService;
  private readonly IEmailService _emailService;
  private const int OtpExpirationMinutes = 5;

  public ResendOtpCommandHandler(
    IUnitOfWork unitOfWork,
    IOtpService otpService,
    IEmailService emailService
  )
  {
    _unitOfWork = unitOfWork;
    _otpService = otpService;
    _emailService = emailService;
  }

  public async Task<OtpResendResponseDto> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
  {
    // 1. Validate OTP session ID
    if (string.IsNullOrWhiteSpace(request.OtpSessionId))
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("OtpSessionId", "OTP session ID is required")
      });
    }

    // 2. Validate existing OTP session (get userId from old session)
    var userId = _otpService.GetUserIdFromSession(request.OtpSessionId);
    
    if (!userId.HasValue)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("OtpSessionId", "Invalid or expired OTP session")
      });
    }

    // 3. Get user information
    var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken)
        ?? throw new NotFoundException("User", userId.Value);
    
    // 5. Invalidate old OTP session
    _otpService.InvalidateOtp(request.OtpSessionId);

    // 6. Generate new OTP
    var (newOtpSessionId, newOtpCode) = _otpService.GenerateOtp(user.UserId);

    // 7. Send new OTP via email
    await _emailService.SendOtpEmailAsync(
        user.Email ?? string.Empty,
        user.UserFullName,
        newOtpCode,
        cancellationToken);

    // 8. Return response
    return new OtpResendResponseDto
    {
      IsOtpResent = true,
      NewOtpSessionId = newOtpSessionId,
      OtpExpiresIn = OtpExpirationMinutes * 60
    };
  }
}