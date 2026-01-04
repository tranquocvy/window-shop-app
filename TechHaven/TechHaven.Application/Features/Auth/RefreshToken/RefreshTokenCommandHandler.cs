using TechHaven.Application.Interfaces;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtTokenService _jwtTokenService;

  public RefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService)
  {
    _unitOfWork = unitOfWork;
    _jwtTokenService = jwtTokenService;
  }

  public async Task<RefreshTokenResponseDto> Handle(
    RefreshTokenCommand request,
    CancellationToken cancellationToken)
  {
    // 1. Validate refresh token format
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("RefreshToken", "Refresh token is required")
      });
    }

    // 2. Find user by refresh token
    var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
    var user = users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

    if (user == null)
    {
      throw new ValidationException(new[]
      {
        new FluentValidation.Results.ValidationFailure("RefreshToken", "Invalid refresh token")
      });
    }

    // 3. Check if refresh token is expired
    if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
    {
      throw new ValidationException(new[]
      {
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

        // 5. Ensure Role is loaded
        if (user.Role == null)
    {
      var userWithRole = (await _unitOfWork.Users.GetWithRoleAsync(cancellationToken))
          .FirstOrDefault(u => u.UserId == user.UserId)
          ?? throw new NotFoundException("User", user.UserId);
      user = userWithRole;
    }

    // 6. Generate new access token and refresh token
    var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
    var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

    // 7. Update user with new refresh token
    user.RefreshToken = newRefreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

    await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // 8. Return new tokens
    return new RefreshTokenResponseDto
    {
      AccessToken = newAccessToken,
      NewRefreshToken = newRefreshToken
    };
  }
}