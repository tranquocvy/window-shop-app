using System;
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

        // map sessionId -> (userId, otpCode)
        private readonly Dictionary<string, (int UserId, string Otp)> _otpSessions = new();

        public async Task<ResponseWrapper<LoginResponseDto>> VerifyLoginAsync(string username, string password)
        {
            var usersResponse = await _userService.GetAllUsersAsync();
            if (!usersResponse.Success || usersResponse.Data == null)
                return new ResponseWrapper<LoginResponseDto> { Success = false, Message = "No users available" };

            var user = usersResponse.Data.FirstOrDefault(u => u.UserName == username);

            if (user == null || !user.IsActive || password != username)
                return new ResponseWrapper<LoginResponseDto> { Success = false, Message = "Invalid username or password" };

            // create a mock OtpSessionId
            var sessionId = "sessionid";
            var otpCode = "000000";
            _otpSessions[sessionId] = (user.UserId, otpCode);

            var loginResponse = new LoginResponseDto
            {
                UserFullName = user.UserFullName ?? string.Empty,
                MaskedEmail = "qu***@***.com",
                RequiresOtp = true,
                OtpExpiresIn = 10,
                OtpSessionId = sessionId
            };

            return new ResponseWrapper<LoginResponseDto> { Success = true, Message = "Login accepted, OTP required", Data = loginResponse };
        }

        public Task<ResponseWrapper<OtpVerifyResponseDto>> VerifyOtpAsync(OtpVerifyRequestDto dto)
        {
            if (!_otpSessions.TryGetValue(dto.OtpSessionId, out var entry))
                return Task.FromResult(new ResponseWrapper<OtpVerifyResponseDto> { Success = false, Message = "OTP session not found" });

            if (entry.Otp == dto.OtpCode)
            {
                _otpSessions.Remove(dto.OtpSessionId);

                // Return token + user info as OtpVerifyResponseDto
                var userResp = _userService.GetUserByIdAsync(entry.UserId).Result;
                if (!userResp.Success || userResp.Data == null)
                {
                    return Task.FromResult(new ResponseWrapper<OtpVerifyResponseDto> { Success = false, Message = "User not found" });
                }

                var user = userResp.Data;
                var otpResp = new OtpVerifyResponseDto
                {
                    AccessToken = "mock-access-token",
                    RefreshToken = "mock-refresh-token",
                    UserName = user.UserName ?? string.Empty,
                    UserFullName = user.UserFullName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    RoleId = user.RoleId,
                    RoleName = user.RoleName ?? string.Empty
                };

                return Task.FromResult(new ResponseWrapper<OtpVerifyResponseDto> { Success = true, Message = "OTP verified", Data = otpResp });
            }

            return Task.FromResult(new ResponseWrapper<OtpVerifyResponseDto> { Success = false, Message = "Invalid OTP" });
        }

        public async Task<ResponseWrapper<OtpResendResponseDto>> ResendOtpAsync(OtpResendRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.OtpSessionId))
                return new ResponseWrapper<OtpResendResponseDto> { Success = false, Message = "Invalid session" };

            if (!_otpSessions.TryGetValue(dto.OtpSessionId, out var entry))
                return new ResponseWrapper<OtpResendResponseDto> { Success = false, Message = "OTP session not found" };

            var newSessionId = dto.OtpSessionId + "-r";
            _otpSessions.Remove(dto.OtpSessionId);
            _otpSessions[newSessionId] = (entry.UserId, "000000");

            var resp = new OtpResendResponseDto
            {
                IsOtpResent = true,
                NewOtpSessionId = newSessionId,
                OtpExpiresIn = 15
            };

            return new ResponseWrapper<OtpResendResponseDto> { Success = true, Message = "OTP resent", Data = resp };
        }

        public Task<ResponseWrapper<SignupResponseDto>> SignupAsync(SignupRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseWrapper<ActivateResponseDto>> ActivateAsync(ActivateRequestDto dto)
        {
            return Task.FromResult(new ResponseWrapper<ActivateResponseDto>
            {
                Success = true,
                Data = new ActivateResponseDto { IsValid = true }
            });
        }

        public Task<ResponseWrapper<IsActiveResponseDto>> CheckTrialStatusAsync()
        {
            return Task.FromResult(new ResponseWrapper<IsActiveResponseDto>
            {
                Success = true,
                Data = new IsActiveResponseDto { IsActive = true, DaysRemain = 30 }
            });
        }
    }
}
