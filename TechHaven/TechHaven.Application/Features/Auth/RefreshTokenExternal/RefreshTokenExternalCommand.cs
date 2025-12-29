using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.RefreshTokenExternal;

public record RefreshTokenExternalCommand(
    string RefreshToken,
    string EncryptedDbConfig)
  : ICommand<RefreshTokenResponseDto>;