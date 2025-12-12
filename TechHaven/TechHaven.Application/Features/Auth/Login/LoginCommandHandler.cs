using TechHaven.Application.Interfaces;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IPasswordHasher _passwordHasher;
  private readonly IOtpService _otpService;
  private readonly IEmailService _emailService;
  private const int OtpExpirationMinutes = 5;

  public LoginCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IOtpService otpService,
    IEmailService emailService)
  {
    _unitOfWork = unitOfWork;
    _passwordHasher = passwordHasher;
    _otpService = otpService;
    _emailService = emailService;
  }

  public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    // 1. Find user by username
    var user = await _unitOfWork.Users.GetByUserNameAsync(request.UserName, cancellationToken)
        ?? throw new NotFoundException("Invalid username or password");

    // 2. Verify password
    if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
    {
      throw new NotFoundException("Invalid username or password");
    }

     // 3. Check if user is active - thêm: check Date.UtcNow - user.createdAt >= 15 days
    if (!user.IsActive && (DateTime.UtcNow - user.CreatedAt).TotalDays >= 15)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("UserName", "User account is deactivated after 15 days of trial mode")
      });
    }

    // 4. Generate OTP and get both session ID and code
    var (otpSessionId, otpCode) = _otpService.GenerateOtp(user.UserId);

    // 5. Send OTP via email
    try
    {
        await _emailService.SendOtpEmailAsync(
            user.Email ?? string.Empty,
            user.UserFullName,
            otpCode,
            cancellationToken);
    }
    catch (Exception ex)
    {
        // Log error but don't fail the login process
        // Consider invalidating OTP if email fails
        _otpService.InvalidateOtp(otpSessionId);
        
        throw new InvalidOperationException(
            "Failed to send OTP email. Please try again later.", ex);
    }

    // 6. Create response with all required fields
    var response = new LoginResponseDto
    {
      UserFullName = user.UserFullName,
      MaskedEmail = MaskEmail(user.Email),
      RequiresOtp = true,
      OtpExpiresIn = OtpExpirationMinutes * 60, // Convert to seconds
      OtpSessionId = otpSessionId
    };

    return response;
  }

  private string MaskEmail(string? email)
  {
    if (string.IsNullOrEmpty(email))
      return "***@***.***";

    var parts = email.Split('@');
    if (parts.Length != 2)
      return "***@***.***";

    var localPart = parts[0];
    var domain = parts[1];

    // Mask local part: show first 2 chars + ***
    var maskedLocal = localPart.Length > 2
      ? localPart.Substring(0, 2) + "***"
      : "***";

    // Mask domain: show first char + *** + extension
    var domainParts = domain.Split('.');
    var maskedDomain = domainParts.Length > 1
      ? domainParts[0][0] + "***." + domainParts[^1]
      : "***";

    return $"{maskedLocal}@{maskedDomain}";
  }
}