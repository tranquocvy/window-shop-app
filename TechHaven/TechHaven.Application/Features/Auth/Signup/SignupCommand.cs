using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.Signup;

public record SignupCommand(
  string UserFullName,
  string Email,
  string UserName,
  string Password,
// string ConfirmPassword,
  int RoleId
) : ICommand<SignupResponseDto>;