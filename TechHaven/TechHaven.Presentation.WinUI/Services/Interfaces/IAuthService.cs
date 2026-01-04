using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseWrapper<LoginResponseDto>> VerifyLoginAsync(string username, string password);
        Task<ResponseWrapper<LoginResponseDto>> VerifyLoginExternalAsync(LoginExternalRequestDto dto);
        Task<ResponseWrapper<OtpVerifyResponseDto>> VerifyOtpAsync(OtpVerifyRequestDto dto);
        Task<ResponseWrapper<OtpResendResponseDto>> ResendOtpAsync(OtpResendRequestDto dto);
        Task<ResponseWrapper<SignupResponseDto>> SignupAsync(SignupRequestDto dto);
        Task<ResponseWrapper<ActivateResponseDto>> ActivateAsync(ActivateRequestDto dto);
        Task<ResponseWrapper<IsActiveResponseDto>> CheckTrialStatusAsync();
    }
}
