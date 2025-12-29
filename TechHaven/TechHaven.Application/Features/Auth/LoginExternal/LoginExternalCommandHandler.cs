using MediatR;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Auth;
//using FluentValidation; // Lưu ý namespace này để dùng ValidationException

namespace TechHaven.Application.Features.Auth.LoginExternal;

public class LoginExternalCommandHandler : IRequestHandler<LoginExternalCommand, LoginResponseDto>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IExternalAuthService _externalAuthService; // <--- Interface mới
    private readonly IStringEncryptionHelper _stringEncryptionHelper;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private const int OtpExpirationMinutes = 5;

    public LoginExternalCommandHandler(
        IPasswordHasher passwordHasher,
        IExternalAuthService externalAuthService,
        IStringEncryptionHelper stringEncryptionHelper,
        IOtpService otpService,
        IEmailService emailService
        )
    {
        _passwordHasher = passwordHasher;
        _externalAuthService = externalAuthService;
        _stringEncryptionHelper = stringEncryptionHelper;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<LoginResponseDto> Handle(LoginExternalCommand request, CancellationToken cancellationToken)
    {
        // 1. Nhờ Service build chuỗi kết nối chuẩn (An toàn, không lo sai cú pháp)
        var connectionString = _externalAuthService.BuildConnectionString(
            request.DbHost,
            request.DbPort,
            request.DbName,
            request.DbUser,
            request.DbPass
        );

        // 2. Kiểm tra kết nối (Gọi qua Interface)
        // Lưu ý: Có thể bỏ qua bước này và gộp vào bước lấy User để tiết kiệm 1 round-trip kết nối
        // Nhưng tách ra thì báo lỗi chi tiết hơn.
        bool canConnect = await _externalAuthService.TestConnectionAsync(connectionString);
        if (!canConnect)
        {
            throw new ValidationException(new[] {
                new FluentValidation.Results.ValidationFailure("Database", "Cannot connect to the specified database.")
            });
        }

        // 3. Lấy User từ External DB (Qua Interface)
        var user = await _externalAuthService.GetUserFromExternalDbAsync(connectionString, request.UserName);

        if (user == null)
        {
            throw new NotFoundException("Invalid username or password in external database");
        }

        // 4. Verify Password (Logic nghiệp vụ - Giữ ở Application)
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new NotFoundException("Invalid username or password");
        }

        // 5. Check Active/Trial (Logic nghiệp vụ - Giữ ở Application)
        //TODO: Tạm thời không kiểm tra ở Backend mà để Frontend xử lý
        if (!user.IsActive && (DateTime.UtcNow - user.CreatedAt).TotalDays >= 15)
        {
            //throw new ValidationException(new[] {  new FluentValidation.Results.ValidationFailure("User", "Trial expired.")});
        }

        // 6. Generate OTP
        var (otpSessionId, otpCode) = _otpService.GenerateOtp(user.UserId);

        // 7. Send OTP via email
        try
        {
            await _emailService.SendOtpEmailAsync(
                user.Email ?? string.Empty,
                user.UserFullName,
                otpCode,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Nếu gửi mail lỗi thì hủy session OTP để user không bị kẹt
            _otpService.InvalidateOtp(otpSessionId);

            // Log error (nếu có ILogger) và ném lỗi ra ngoài
            throw new InvalidOperationException("Failed to send OTP email. Please try again later.", ex);
        }
        // 8. Mã hóa & Tạo Token
        var encryptedDbConfig = _stringEncryptionHelper.Encrypt(connectionString);

        // 9. Trả về Response
        return new LoginResponseDto
        {
            UserFullName = user.UserFullName,
            MaskedEmail = MaskEmail(user.Email),
            RequiresOtp = true,
            OtpSessionId = otpSessionId,
            OtpExpiresIn = OtpExpirationMinutes * 60,

            // QUAN TRỌNG: Chúng ta dùng trường EncryptDbConfig để chứa Config mã hóa.
            // Client phải lấy giá trị này gửi vào field "encryptedDbConfig" của API verify-otp-external
            EncryptedDbConfig = encryptedDbConfig
        };
    }
    // Helper function để che email (Copy từ LoginCommandHandler)
    private string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return "***@***.***";
        var parts = email.Split('@');
        if (parts.Length != 2) return "***@***.***";
        var localPart = parts[0];
        var domain = parts[1];
        var maskedLocal = localPart.Length > 2 ? localPart.Substring(0, 2) + "***" : "***";
        var domainParts = domain.Split('.');
        var maskedDomain = domainParts.Length > 1 ? domainParts[0][0] + "***." + domainParts[^1] : "***";
        return $"{maskedLocal}@{maskedDomain}";
    }
}