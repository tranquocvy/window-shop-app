using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs.Common;
using TechHaven.Presentation.WinUI.Services.Interfaces;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/auth";

        public HttpAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ResponseWrapper<LoginResponseDto>> VerifyLoginAsync(string username, string password)
        {
            var payload = new { Username = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/login", payload);
            return await response.EnsureSuccessAndReadWrapperAsync<LoginResponseDto>("Failed to verify login");
        }

        public async Task<ResponseWrapper<bool>> VerifyOtpAsync(OtpVerifyRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/verify-otp", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to verify OTP");
        }

        public async Task<ResponseWrapper<bool>> ResendOtpAsync(int userId)
        {
            // send an empty object body to trigger resend on server
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/resend-otp/{userId}", new { });
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to resend OTP");
        }

        public Task<ResponseWrapper<UserDto>> GetUserDtoAsync(int userId)
        {
            return _httpClient.GetWrapperFromJsonAsync<UserDto>($"{BaseUrl}/users/{userId}", "Failed to retrieve user");
        }
    }
}