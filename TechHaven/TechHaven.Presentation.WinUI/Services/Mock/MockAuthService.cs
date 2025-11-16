using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Users;
using TechHaven.Shared.DTOs.Auth;
using Shared.DTOs.Common;

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
            {
                return new ResponseWrapper<LoginResponseDto>
                {
                    Success = false,
                    Message = "Unable to retrieve user list."
                };
            }

            var user = usersResponse.Data.FirstOrDefault(u => u.UserName == username);

            if (user == null || !user.IsActive)
            {
                return new ResponseWrapper<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            if (password != username)
            {
                return new ResponseWrapper<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

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

            return new ResponseWrapper<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful. Please verify OTP.",
                Data = response
            };
        }

        public Task<ResponseWrapper<bool>> VerifyOtpAsync(OtpVerifyRequestDto dto)
        {
            if (!_otpStore.TryGetValue(dto.UserId, out var entry))
            {
                return Task.FromResult(new ResponseWrapper<bool>
                {
                    Success = false,
                    Message = "OTP does not exist or has expired.",
                    Data = false
                });
            }

            if (entry == dto.OtpCode)
            {
                _otpStore.Remove(dto.UserId);
                return Task.FromResult(new ResponseWrapper<bool>
                {
                    Success = true,
                    Message = "OTP verified successfully.",
                    Data = true
                });
            }

            return Task.FromResult(new ResponseWrapper<bool>
            {
                Success = false,
                Message = "Invalid OTP code.",
                Data = false
            });
        }

        public async Task<ResponseWrapper<bool>> ResendOtpAsync(int userId)
        {
            var userResponse = await _userService.GetUserByIdAsync(userId);
            if (!userResponse.Success || userResponse.Data == null)
            {
                return new ResponseWrapper<bool>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = false
                };
            }

            _otpStore[userId] = "000000";
            return new ResponseWrapper<bool>
            {
                Success = true,
                Message = "OTP has been resent.",
                Data = true
            };
        }

        public async Task<ResponseWrapper<UserDto>> GetUserDtoAsync(int userId)
        {
            var userResponse = await _userService.GetUserByIdAsync(userId);
            
            if (!userResponse.Success || userResponse.Data == null)
            {
                return new ResponseWrapper<UserDto>
                {
                    Success = false,
                    Message = "User information not found."
                };
            }

            return new ResponseWrapper<UserDto>
            {
                Success = true,
                Message = "User information retrieved successfully.",
                Data = userResponse.Data
            };
        }
    }
}
