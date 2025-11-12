using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.VerifyOtp;

public record VerifyOtpCommand(int UserId, string OtpCode) : ICommand<OtpVerifyResponseDto>;