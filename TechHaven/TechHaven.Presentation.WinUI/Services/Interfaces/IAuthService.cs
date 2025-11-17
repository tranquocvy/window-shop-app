using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseWrapper<LoginResponseDto>> VerifyLoginAsync(string username, string password);
        Task<ResponseWrapper<bool>> VerifyOtpAsync(OtpVerifyRequestDto dto);
        Task<ResponseWrapper<bool>> ResendOtpAsync(int userId);
        Task<ResponseWrapper<UserDto>> GetUserDtoAsync(int userId);
    }
}
