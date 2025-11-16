using AutoMapper;
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
  private readonly IMapper _mapper;

  public LoginCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IOtpService otpService,
    IEmailService emailService,
    IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _passwordHasher = passwordHasher;
    _otpService = otpService;
    _emailService = emailService;
    _mapper = mapper;
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

    // 3. Check if user is active
    if (!user.IsActive)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("UserName", "User account is deactivated")
      });
    }

    // 4. Generate OTP
    var otpCode = _otpService.GenerateOtp(user.UserId);

    // 5. Send OTP via email
    await _emailService.SendOtpEmailAsync(
        user.Email ?? string.Empty,
        user.UserFullName,
        otpCode);

    // 6. Map to response DTO
    var response = _mapper.Map<LoginResponseDto>(user);
    response.RequiresOtp = true; // Indicate that OTP verification is needed

    return response;
  }
}