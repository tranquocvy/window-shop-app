using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.VerifyOtp;

public record VerifyOtpCommand(string OtpSessionId, string OtpCode) : ICommand<OtpVerifyResponseDto>;