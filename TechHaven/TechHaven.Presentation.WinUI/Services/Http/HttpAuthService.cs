using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/Auth";

        public HttpAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ResponseWrapper<LoginResponseDto>> VerifyLoginAsync(string username, string password)
        {
            var payload = new LoginRequestDto { UserName = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/login", payload);
            return await response.EnsureSuccessAndReadWrapperAsync<LoginResponseDto>("Failed to verify login");
        }

        public async Task<ResponseWrapper<OtpVerifyResponseDto>> VerifyOtpAsync(OtpVerifyRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/verify-otp", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<OtpVerifyResponseDto>("Failed to verify OTP");
        }

        public async Task<ResponseWrapper<OtpResendResponseDto>> ResendOtpAsync(OtpResendRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/resend-otp", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<OtpResendResponseDto>("Failed to resend OTP");
        }

        public async Task<ResponseWrapper<SignupResponseDto>> SignupAsync(SignupRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/signup", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<SignupResponseDto>("Failed to signup user");
        }

        public async Task<ResponseWrapper<ActivateResponseDto>> ActivateAsync(ActivateRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/activate", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<ActivateResponseDto>("Failed to activate account");
        }

        public async Task<ResponseWrapper<IsActiveResponseDto>> CheckTrialStatusAsync()
        {
            return await _httpClient.GetWrapperFromJsonAsync<IsActiveResponseDto>($"{BaseUrl}/isActive", "Failed to check trial status");
        }
    }
}