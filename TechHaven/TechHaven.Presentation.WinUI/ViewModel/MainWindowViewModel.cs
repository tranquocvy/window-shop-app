using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Presentation.WinUI.Services.Http;
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
        private int _userId = 0;

        public MainWindowViewModel()    
        {
            // Use HttpAuthService (calls backend)
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };
            _authService = new HttpAuthService(httpClient);
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

                // Store user ID and check if OTP is required
                UserId = result.Data.UserId;
                RequiresOtp = result.Data.RequiresOtp;

                if (!RequiresOtp)
                {
                    // If no OTP required, set current user from login response
                    Helpers.AppState.CurrentUser = new UserDto
                    {
                        UserId = result.Data.UserId,
                        UserName = result.Data.UserName,
                        UserFullName = result.Data.UserFullName,
                        RoleId = result.Data.RoleId,
                        RoleName = result.Data.RoleName,
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
            if (UserId == 0)
                return false;

            ErrorMessage = string.Empty;

            try
            {
                var dto = new OtpVerifyRequestDto
                {
                    UserId = UserId,
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
                Helpers.AppState.CurrentUser = new UserDto
                {
                    UserId = otpData.UserId,
                    UserName = otpData.UserName,
                    UserFullName = otpData.UserFullName,
                    RoleId = 0, // role id not provided in OtpVerifyResponseDto
                    RoleName = otpData.RoleName,
                    IsActive = true
                };

                // Optionally store access token somewhere if needed (not implemented)

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
            if (UserId == 0)
                return;

            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.ResendOtpAsync(UserId);

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
            // Return true if AppState.CurrentUser is set and matches the logged in user
            if (Helpers.AppState.CurrentUser != null && Helpers.AppState.CurrentUser.UserId == UserId)
                return Task.FromResult(true);

            // No user available
            ErrorMessage = "User information not available.";
            return Task.FromResult(false);
        }
    }
}
