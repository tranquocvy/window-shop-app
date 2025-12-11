using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private static readonly HttpClient SharedHttpClient = ApiClientFactory.GetHttpClient();
        private readonly IAuthService _authService;
        private DispatcherQueue? _dispatcher;
        private Timer? _timer;

        private string _otpSessionIdInternal = string.Empty;
        private int _otpExpiresInInternal = 0;

        // Use http-based auth service by default, if you want to use mock, uncomment the other constructor
        public MainWindowViewModel() : this(new HttpAuthService(SharedHttpClient)) { }
        //public MainWindowViewModel() : this(new MockAuthService()) { }

        public MainWindowViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            LoginCommand = new AsyncRelayCommand(LoginAsync);
            VerifyOtpCommand = new AsyncRelayCommand(VerifyOtpExecute, () => IsVerifyEnabled);
            ResendOtpCommand = new AsyncRelayCommand(ResendOtpAsync);
            CloseOtpCommand = new RelayCommand(CloseOtpExecute);
        }

        // Commands
        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand VerifyOtpCommand { get; }
        public IAsyncRelayCommand ResendOtpCommand { get; }
        public ICommand CloseOtpCommand { get; }

        // Observable properties (source-generator reduces boilerplate)
        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool requiresOtp = false;

        // Single display string for OTP info (greeting + sent text)
        [ObservableProperty]
        private string otpInfo = string.Empty;

        [ObservableProperty]
        private int otpRemaining = 0;

        [ObservableProperty]
        private string otpInput = string.Empty;

        [ObservableProperty]
        private bool isVerifyEnabled = true;

        // Remember me
        [ObservableProperty]
        private bool rememberMe = false;

        // Called by source-generator when IsVerifyEnabled changes
        partial void OnIsVerifyEnabledChanged(bool value)
        {
            (VerifyOtpCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username)) { ErrorMessage = "Please enter username."; return; }
            if (string.IsNullOrWhiteSpace(Password)) { ErrorMessage = "Please enter password."; return; }

            try
            {
                IsVerifyEnabled = true; // reset state
                var result = await _authService.VerifyLoginAsync(Username, Password);
                if (!result.Success || result.Data == null) { ErrorMessage = result.Message ?? "Login failed."; return; }

                _otpSessionIdInternal = result.Data.OtpSessionId ?? string.Empty;
                RequiresOtp = result.Data.RequiresOtp;

                // Build single OTP info string (simple)
                var name = result.Data.UserFullName ?? string.Empty;
                var email = result.Data.MaskedEmail ?? string.Empty;
                OtpInfo = string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email) ? string.Empty : $"Xin chào {name}. Đã gửi OTP tới {email}".Trim();

                // Set remaining seconds directly and store expiry for resend
                _otpExpiresInInternal = result.Data.OtpExpiresIn;
                OtpRemaining = _otpExpiresInInternal;

                if (RequiresOtp)
                {
                    StartCountdown();
                    return;
                }

                Helpers.AppState.CurrentUser = new UserDto
                {
                    UserId = 0,
                    UserName = Username,
                    UserFullName = name,
                    RoleId = 0,
                    RoleName = string.Empty,
                    IsActive = true
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private void StartCountdown()
        {
            _dispatcher = DispatcherQueue.GetForCurrentThread();
            _timer?.Stop(); _timer?.Dispose();

            // OtpRemaining already set by LoginAsync or ResendOtpAsync
            IsVerifyEnabled = OtpRemaining > 0;

            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) =>
            {
                if (_dispatcher != null) _dispatcher.TryEnqueue(Tick);
                else Tick();
            };
            _timer.Start();
        }

        private void Tick()
        {
            OtpRemaining = Math.Max(0, OtpRemaining - 1);
            if (OtpRemaining <= 0)
            {
                IsVerifyEnabled = false;
                _timer?.Stop();
            }
        }

        private void StopCountdown()
        {
            _timer?.Stop(); _timer?.Dispose(); _timer = null;
        }

        private async Task VerifyOtpExecute()
        {
            IsVerifyEnabled = false;
            var ok = await VerifyOtpInternal(OtpInput?.Trim() ?? string.Empty);
            if (ok)
            {
                StopCountdown();
                RequiresOtp = false;
                OtpInput = string.Empty;
                OtpInfo = string.Empty;
            }
            else
            {
                IsVerifyEnabled = OtpRemaining > 0;
            }
        }

        private void CloseOtpExecute()
        {
            StopCountdown();
            RequiresOtp = false;
        }

        private async Task ResendOtpAsync()
        {
            if (string.IsNullOrWhiteSpace(_otpSessionIdInternal)) return;
            ErrorMessage = string.Empty;

            var dto = new OtpResendRequestDto { OtpSessionId = _otpSessionIdInternal };
            var result = await _authService.ResendOtpAsync(dto);
            if (!result.Success || result.Data == null) { ErrorMessage = result.Message ?? "Unable to resend OTP."; return; }

            var resp = result.Data;
            if (!resp.IsOtpResent) { ErrorMessage = result.Message ?? "Unable to resend OTP."; return; }

            // Update internal session id and expiry from response
            if (!string.IsNullOrWhiteSpace(resp.NewOtpSessionId))
            {
                _otpSessionIdInternal = resp.NewOtpSessionId;
            }

            // Use returned expiry if available, otherwise keep previous
            if (resp.OtpExpiresIn > 0)
            {
                _otpExpiresInInternal = resp.OtpExpiresIn;
                OtpRemaining = _otpExpiresInInternal;
                StartCountdown();
            }

            IsVerifyEnabled = OtpRemaining > 0;
        }

        private async Task<bool> VerifyOtpInternal(string otpCode)
        {
            if (string.IsNullOrWhiteSpace(_otpSessionIdInternal)) return false;
            ErrorMessage = string.Empty;

            var dto = new OtpVerifyRequestDto { OtpSessionId = _otpSessionIdInternal, OtpCode = otpCode };
            var result = await _authService.VerifyOtpAsync(dto);
            if (!result.Success || result.Data == null) { ErrorMessage = result.Message ?? "Invalid OTP code."; return false; }

            var otpData = result.Data;
            TokenStore.AccessToken = otpData.AccessToken;
            TokenStore.RefreshToken = otpData.RefreshToken;

            // persist refresh token if requested
            try
            {
                if (!string.IsNullOrWhiteSpace(TokenStore.RefreshToken))
                {
                    TokenPersistence.SaveRefreshToken(TokenStore.RefreshToken);
                }
            }
            catch { }

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

        public Task<bool> CompleteLoginAndGetUserAsync()
        {
            if (Helpers.AppState.CurrentUser != null) return Task.FromResult(true);
            ErrorMessage = "User information not available.";
            return Task.FromResult(false);
        }
    }
}
