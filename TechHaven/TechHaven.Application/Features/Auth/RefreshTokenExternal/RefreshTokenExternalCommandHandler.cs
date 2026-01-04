using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.RefreshToken;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.RefreshTokenExternal;

public class RefreshTokenExternalCommandHandler : ICommandHandler<RefreshTokenExternalCommand, RefreshTokenResponseDto>
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly IStringEncryptionHelper _encryptionHelper;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenExternalCommandHandler(
          IExternalAuthService externalAuthService,
          IStringEncryptionHelper encryptionHelper,
          IJwtTokenService jwtTokenService)
    {
        _externalAuthService = externalAuthService;
        _encryptionHelper = encryptionHelper;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RefreshTokenResponseDto> Handle(
    RefreshTokenExternalCommand request,
    CancellationToken cancellationToken)
  {
     // 1. Giải mã Connection String
        string connectionString;
        try
        {
            connectionString = _encryptionHelper.Decrypt(request.EncryptedDbConfig);
        }
        catch
        {
            throw new ValidationException(new[] {
                new FluentValidation.Results.ValidationFailure("EncryptedDbConfig", "Invalid database configuration")
            });
        }
        // 2. Tìm User bằng Refresh Token trên External DB
        var user = await _externalAuthService.GetUserByRefreshTokenFromExternalDbAsync(connectionString, request.RefreshToken);

        if (user == null)
        {
            throw new ValidationException(new[] {
                new FluentValidation.Results.ValidationFailure("RefreshToken", "Invalid refresh token")
            });
        }

        // 3. Kiểm tra Token hết hạn chưa
        if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new ValidationException(new[] {
                new FluentValidation.Results.ValidationFailure("RefreshToken", "Refresh token has expired")
            });
        }

        

        // 4. Check if user is still active
        /*
          Tạm thời bỏ qua người dùng có là isActive hay ko, vì ta vẫn sẽ để người dùng đăng nhập thành công. Nhưng sẽ chặn các hành động khác trừ khi họ nâng cấp tài khoản.
        //TODO: có 2 hướng dể xử lý
         
        1. Coi is_active là trường để thể hiện free trial hay official account ?
          -> thì  ta sẽ cần một trường mới là isValidAccount để kiểm tra tài khoản có bị ban bởi admin không ?
          --> cái này tạm thời có thể chưa cần  trong đồ án này
          --> Ở TH1 thì ta không cần giữ logic bên dưới hđ, (do FE đề xuất) vì nhằm mục đích để user log vào hệ thống kiểm tra tài khoản có còn trong thời gian dùng thử hay không 
         2. Coi is_active là trường để thể hiện tài khoản có bị ban hay không ?
          -> thì ta sẽ cần một trường mới là accountType để phân biệt tài khoản free trial hay official account ?
          --> Như này thì ta buộc phải cài thêm và Db migration -> hơi mất thời gian xíu
          -> ở TH2 thì ta hoàn toàn có thể giữ logic bên dưới hoạt động - vì acc bị ban thì ko cho đăng nhập
        */

        // if (!user.IsActive)
        //{
        //  throw new ValidationException(new[]
        //  {
        //    new FluentValidation.Results.ValidationFailure("User", "User account is deactivated after 15 days of trial mode")
        //  });
        //}

    // 5. Generate new access token and refresh token
    var newAccessToken = _jwtTokenService.GenerateAccessToken(user, request.EncryptedDbConfig); //AccessToken mới PHẢI chứa EncryptedDbConfig để các request sau đó hoạt động
    var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

    // 6. Cập nhật Token mới vào DB External
        // (Hàm này bạn đã có từ lúc làm VerifyOtpExternal)
    await _externalAuthService.UpdateRefreshTokenAsync(
         connectionString, 
         user.UserId, 
         newRefreshToken, 
         DateTime.UtcNow.AddDays(7)
         );

    // 8. Return new tokens
    return new RefreshTokenResponseDto
    {
      AccessToken = newAccessToken,
      NewRefreshToken = newRefreshToken
    };
  }
}