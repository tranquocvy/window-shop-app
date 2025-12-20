using Microsoft.Extensions.Logging;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.Signup;

public class SignupCommandHandler : ICommandHandler<SignupCommand, SignupResponseDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<SignupCommandHandler> _logger;

  public SignupCommandHandler(
      IUnitOfWork unitOfWork,
      ILogger<SignupCommandHandler> logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<SignupResponseDto> Handle(
      SignupCommand request,
      CancellationToken cancellationToken)
  {
    // // Validate passwords match
    // if (request.Password != request.ConfirmPassword)
    // {
    //   throw new ValidationException("Passwords do not match");
    // }

    // Check if username already exists
    var existingUserByUsername = await _unitOfWork.Users.FirstOrDefaultAsync(
        u => u.UserName == request.UserName,
        cancellationToken);

    if (existingUserByUsername != null)
    {
      throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
      {
          new FluentValidation.Results.ValidationFailure(nameof(request.UserName), $"Username '{request.UserName}' is already taken")
      });
    }

    // // Check if email already exists
    // var existingUserByEmail = await _unitOfWork.Users.FirstOrDefaultAsync(
    //     u => u.Email == request.Email,
    //     cancellationToken);

    // if (existingUserByEmail != null)
    // {
    //   throw new ValidationException(new List<FluentValidation.Results.ValidationFailure>
    //   {
    //       new FluentValidation.Results.ValidationFailure(nameof(request.Email), $"Email '{request.Email}' is already registered")
    //   });
    // }

    // Create new user with Seller role (RoleId = 2)
    var newUser = new User
    {
      UserFullName = request.UserFullName,
      Email = request.Email,
      UserName = request.UserName,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
      RoleId = request.RoleId,
      IsActive = true,
      HasSeenGuide = false,
      CreatedAt = DateTime.UtcNow,
      ActivatedAt = DateTime.UtcNow
    };

    await _unitOfWork.Users.AddAsync(newUser, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation(
        "New user registered successfully: UserId={UserId}, UserName={UserName}",
        newUser.UserId,
        newUser.UserName);

    return new SignupResponseDto
    {
      UserId = newUser.UserId,
      UserFullName = newUser.UserFullName,
      UserName = newUser.UserName,
      RoleId = newUser.RoleId,
      Email = newUser.Email ?? string.Empty,
      Message = "Account created successfully. You can now login."
    };
  }
}