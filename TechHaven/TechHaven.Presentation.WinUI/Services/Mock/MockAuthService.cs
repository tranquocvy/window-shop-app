using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockAuthService : IAuthService
    {
        private readonly MockUserService _userService = new();
        private readonly Dictionary<int, string> _otpStore = new();

        public async Task<ResponseWrapper<LoginResponseDto>> VerifyLoginAsync(string username, string password)
        {
            var usersResponse = await _userService.GetAllUsersAsync();
            if (!usersResponse.Success || usersResponse.Data == null)
                return new ResponseWrapper<LoginResponseDto> { Success = false, Message = "No users available" };

            var user = usersResponse.Data.FirstOrDefault(u => u.UserName == username);

            if (user == null || !user.IsActive || password != username)
                return new ResponseWrapper<LoginResponseDto> { Success = false, Message = "Invalid username or password" };

            var loginResponse = new LoginResponseDto
            {
                UserId = user.UserId,
                UserName = user.UserName ?? string.Empty,
                UserFullName = user.UserFullName ?? string.Empty,
                RoleId = user.RoleId,
                RoleName = user.RoleName ?? string.Empty,
                RequiresOtp = true
            };

            _otpStore[user.UserId] = "000000";

            return new ResponseWrapper<LoginResponseDto> { Success = true, Message = "Login accepted, OTP required", Data = loginResponse };
        }

        public Task<ResponseWrapper<OtpVerifyResponseDto>> VerifyOtpAsync(OtpVerifyRequestDto dto)
        {
            if (!_otpStore.TryGetValue(dto.UserId, out var entry))
                return Task.FromResult(new ResponseWrapper<OtpVerifyResponseDto> { Success = false, Message = "OTP not found" });

            if (entry == dto.OtpCode)
            {
                _otpStore.Remove(dto.UserId);

                // Return token + user info as OtpVerifyResponseDto
                var user = _userService.GetUserByIdAsync(dto.UserId).Result.Data!;
                var otpResp = new OtpVerifyResponseDto
                {
                    AccessToken = "mock-token",
                    UserId = user.UserId,
                    UserName = user.UserName ?? string.Empty,
                    UserFullName = user.UserFullName ?? string.Empty,
                    RoleName = user.RoleName ?? string.Empty
                };

                return Task.FromResult(new ResponseWrapper<OtpVerifyResponseDto> { Success = true, Message = "OTP verified", Data = otpResp });
            }

            return Task.FromResult(new ResponseWrapper<OtpVerifyResponseDto> { Success = false, Message = "Invalid OTP" });
        }

        public async Task<ResponseWrapper<bool>> ResendOtpAsync(int userId)
        {
            var userResponse = await _userService.GetUserByIdAsync(userId);
            if (!userResponse.Success || userResponse.Data == null)
                return new ResponseWrapper<bool> { Success = false, Message = "User not found" };

            _otpStore[userId] = "000000";
            return new ResponseWrapper<bool> { Success = true, Message = "OTP resent", Data = true };
        }

        public async Task<ResponseWrapper<UserDto>> GetUserDtoAsync(int userId)
        {
            var userResponse = await _userService.GetUserByIdAsync(userId);
            if (!userResponse.Success)
                return new ResponseWrapper<UserDto> { Success = false, Message = userResponse.Message };

            return new ResponseWrapper<UserDto> { Success = true, Message = "User retrieved", Data = userResponse.Data };
        }
    }
}
