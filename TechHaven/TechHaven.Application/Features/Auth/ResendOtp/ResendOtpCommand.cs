using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.ResendOtp;

public record ResendOtpCommand(string OtpSessionId): ICommand<OtpResendResponseDto>;