using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.VerifyOtpExternal;

// Client phải gửi kèm EncryptedDbConfig nhận được từ bước Login
public record VerifyOtpExternalCommand(
    string OtpSessionId,
    string OtpCode,
    string EncryptedDbConfig
) : ICommand <OtpVerifyResponseDto>;