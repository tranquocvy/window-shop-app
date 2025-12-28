using MediatR;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.VerifyOtpExternal;
public class VerifyOtpExternalCommandHandler : IRequestHandler<VerifyOtpExternalCommand, OtpVerifyResponseDto>
{
    private readonly IOtpService _otpService;
    private readonly IExternalAuthService _externalAuthService; // Dùng cái này thay UnitOfWork
    private readonly IStringEncryptionHelper _encryptionHelper;
    private readonly IJwtTokenService _jwtTokenService;

    public VerifyOtpExternalCommandHandler(
            IOtpService optService,
            IExternalAuthService externalAuthService,
            IStringEncryptionHelper encryptionHelper,
            IJwtTokenService jwtTokenService
        ) 
    {
        _otpService = optService;
        _externalAuthService = externalAuthService;
        _encryptionHelper = encryptionHelper;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<OtpVerifyResponseDto> Handle(VerifyOtpExternalCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate OTP (Logic cũ, dùng Redis/MemoryCache nên không quan tâm DB nào)
        var userId = _otpService.ValidateOtp(request.OtpSessionId, request.OtpCode);
        if (!userId.HasValue)
        {
            throw new ValidationException(new[]
              {
                new FluentValidation.Results.ValidationFailure("OtpCode", "Invalid or expired OTP code")
              });
        }

        // 2. Giải mã Connection String
        string connectionString = _encryptionHelper.Decrypt(request.EncryptedDbConfig);

        // 3. Lấy User từ External DB (Dùng Service riêng)
        // Bạn cần bổ sung hàm GetUserByIdAsync vào IExternalAuthService
        var user = await _externalAuthService.GetUserByIdFromExternalDbAsync(connectionString, userId.Value);

        if (user == null)
        {
            throw new NotFoundException("User", userId.Value);
        }

        // 4. Invalidate OTP
        _otpService.InvalidateOtp(request.OtpSessionId);

        // 5. Generate Tokens
        // LƯU Ý QUAN TRỌNG: Token này PHẢI chứa claim 'db_config' = request.EncryptedDbConfig
        // Để các request sau (như /me, /orders) biết đường mà tìm DB.
        var accessToken = _jwtTokenService.GenerateAccessToken(user, request.EncryptedDbConfig);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // 6. Save Refresh Token vào External DB - vì không sử dụng UnitOfWork để lưu nữa
        // Bạn cần bổ sung hàm UpdateRefreshTokenAsync vào IExternalAuthService
        await _externalAuthService.UpdateRefreshTokenAsync(connectionString, user.UserId, refreshToken, DateTime.UtcNow.AddDays(7));

        return new OtpVerifyResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserName = user.UserName,
            UserFullName = user.UserFullName,
            Email = user?.Email ?? string.Empty,
            RoleId = user!.RoleId,
            RoleName = user.Role?.RoleName ?? string.Empty,
            HasSeenGuide = user.HasSeenGuide
        };
    }
}