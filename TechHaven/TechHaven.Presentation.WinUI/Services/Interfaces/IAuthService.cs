using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using Shared.DTOs.Common;

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
