using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class LoginViewModel : ObservableObject
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

        public LoginViewModel()
        {
            // Use MockAuthService for now
            _authService = new MockAuthService();
        }

        public LoginViewModel(IAuthService authService)
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
                    // If no OTP required, proceed to get user info and complete login
                    await CompleteLoginAsync();
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

                if (!result.Success || !result.Data)
                {
                    ErrorMessage = result.Message ?? "Invalid OTP code.";
                    return false;
                }

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

        private async Task CompleteLoginAsync()
        {
            var userResult = await _authService.GetUserDtoAsync(UserId);

            if (!userResult.Success || userResult.Data == null)
            {
                ErrorMessage = userResult.Message ?? "User information not found.";
                return;
            }

            // Set user to AppState (will be done in MainWindow after this)
            // This method is for internal use, the MainWindow will handle navigation
        }

        public async Task<bool> CompleteLoginAndGetUserAsync()
        {
            var userResult = await _authService.GetUserDtoAsync(UserId);

            if (!userResult.Success || userResult.Data == null)
            {
                ErrorMessage = userResult.Message ?? "User information not found.";
                return false;
            }

            // Set user to AppState
            Helpers.AppState.CurrentUser = userResult.Data;
            return true;
        }
    }
}
