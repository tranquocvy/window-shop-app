using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseWrapper<LoginResponseDto>> VerifyLoginAsync(string username, string password);
        Task<ResponseWrapper<OtpVerifyResponseDto>> VerifyOtpAsync(OtpVerifyRequestDto dto);
        Task<ResponseWrapper<bool>> ResendOtpAsync(int userId);
    }
}
