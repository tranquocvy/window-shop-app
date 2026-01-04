using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken)
  : ICommand<RefreshTokenResponseDto>;