using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.ResendOtpExternal;

// Client phải gửi kèm Config mã hóa để server biết connect vào đâu
public record ResendOtpExternalCommand(
    string OtpSessionId,
    string EncryptedDbConfig
) : ICommand<OtpResendResponseDto>;