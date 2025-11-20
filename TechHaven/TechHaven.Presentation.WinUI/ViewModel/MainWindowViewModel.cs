using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        // Properties for binding
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        // Properties for OTP flow
        [ObservableProperty]
        private bool _requiresOtp = false;

        [ObservableProperty]
        private string _otpSessionId = string.Empty;

        public MainWindowViewModel()
        {
            // Use MockAuthService for testing while DB/backend not ready
            _authService = new MockAuthService();

            // If you want to test against the real API, use ApiClientFactory.GetHttpClient() and HttpAuthService:
            // var httpClient = ApiClientFactory.GetHttpClient();
            // _authService = new HttpAuthService(httpClient);
        }

        public MainWindowViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            // Clear previous error
            ErrorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Please enter username.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter password.";
                return;
            }

            IsLoading = true;

            try
            {
                var result = await _authService.VerifyLoginAsync(Username, Password);

                if (!result.Success || result.Data == null)
                {
                    ErrorMessage = result.Message ?? "Login failed.";
                    return;
                }

                // Store otp session and check if OTP is required
                _otpSessionId = result.Data.OtpSessionId ?? string.Empty;
                RequiresOtp = result.Data.RequiresOtp;

                if (!RequiresOtp)
                {
                    // If no OTP required, set current user from login response
                    Helpers.AppState.CurrentUser = new UserDto
                    {
                        UserId = 0,
                        UserName = string.Empty,
                        UserFullName = result.Data.UserFullName,
                        RoleId = 0,
                        RoleName = string.Empty,
                        IsActive = true
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> VerifyOtpAsync(string otpCode)
        {
            if (string.IsNullOrWhiteSpace(_otpSessionId))
                return false;

            ErrorMessage = string.Empty;

            try
            {
                var dto = new OtpVerifyRequestDto
                {
                    OtpSessionId = _otpSessionId,
                    OtpCode = otpCode?.Trim() ?? string.Empty
                };

                var result = await _authService.VerifyOtpAsync(dto);

                if (!result.Success || result.Data == null)
                {
                    ErrorMessage = result.Message ?? "Invalid OTP code.";
                    return false;
                }

                // Map OtpVerifyResponseDto to UserDto and set AppState
                var otpData = result.Data;

                // store tokens in TokenStore for subsequent requests
                TokenStore.AccessToken = otpData.AccessToken;
                TokenStore.RefreshToken = otpData.RefreshToken;

                Helpers.AppState.CurrentUser = new UserDto
                {
                    UserId = 0,
                    UserName = otpData.UserName,
                    UserFullName = otpData.UserFullName,
                    RoleId = otpData.RoleId,
                    RoleName = otpData.RoleName,
                    IsActive = true
                };

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        [RelayCommand]
        private async Task ResendOtpAsync()
        {
            if (string.IsNullOrWhiteSpace(_otpSessionId))
                return;

            ErrorMessage = string.Empty;

            try
            {
                var dto = new OtpResendRequestDto { OtpSessionId = _otpSessionId };
                var result = await _authService.ResendOtpAsync(dto);

                if (!result.Success)
                {
                    ErrorMessage = result.Message ?? "Unable to resend OTP.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        public Task<bool> CompleteLoginAndGetUserAsync()
        {
            // Return true if AppState.CurrentUser is set
            if (Helpers.AppState.CurrentUser != null)
                return Task.FromResult(true);

            // No user available
            ErrorMessage = "User information not available.";
            return Task.FromResult(false);
        }
    }
}
