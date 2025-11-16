using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Users;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockAuthService
    {
        private readonly MockUserService _userService = new();
        private readonly Dictionary<int, string> _otpStore = new();

        public async Task<LoginResponseDto?> VerifyLoginAsync(string username, string password)
        {
            var usersResponse = await _userService.GetAllUsersAsync();
            if (!usersResponse.Success || usersResponse.Data == null)
                return null;

            var user = usersResponse.Data.FirstOrDefault(u => u.UserName == username);

            if (user == null || !user.IsActive || password != username)
                return null;

            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                UserName = user.UserName ?? string.Empty,
                UserFullName = user.UserFullName ?? string.Empty,
                RoleId = user.RoleId,
                RoleName = user.RoleName ?? string.Empty,
                RequiresOtp = true
            };

            _otpStore[user.UserId] = "000000";

            return response;
        }

        public Task<bool> VerifyOtpAsync(OtpVerifyRequestDto dto)
        {
            if (!_otpStore.TryGetValue(dto.UserId, out var entry))
                return Task.FromResult(false);

            if (entry == dto.OtpCode)
            {
                _otpStore.Remove(dto.UserId);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public async Task<bool> ResendOtpAsync(int userId)
        {
            var userResponse = await _userService.GetUserByIdAsync(userId);
            if (!userResponse.Success || userResponse.Data == null) 
                return false;

            _otpStore[userId] = "000000";
            return true;
        }

        public async Task<UserDto?> GetUserDtoAsync(int userId)
        {
            var userResponse = await _userService.GetUserByIdAsync(userId);
            return userResponse.Success ? userResponse.Data : null;
        }
    }
}
