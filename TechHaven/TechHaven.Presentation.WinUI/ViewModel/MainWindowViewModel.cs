using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using System;
using System.Linq;
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
        private readonly IUserService _userService;
        private DispatcherQueue? _dispatcher;
        private Timer? _timer;

        private string _otpSessionIdInternal = string.Empty;
        private int _otpExpiresInInternal = 0;
        private string _encryptedDbConfigInternal = string.Empty;

        // Default constructor wires concrete services; prefer DI constructor below in production
        public MainWindowViewModel() : this(new HttpAuthService(SharedHttpClient), new HttpUserService(SharedHttpClient)) { }
        //public MainWindowViewModel() : this(new MockAuthService(), new MockUserService()) { }

        // Use DI to supply services
        public MainWindowViewModel(IAuthService authService, IUserService userService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

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

        // Database configuration properties
        [ObservableProperty]
        private string dbHost = string.Empty;

        [ObservableProperty]
        private string dbPort = string.Empty;

        [ObservableProperty]
        private string dbName = string.Empty;

        [ObservableProperty]
        private string dbUser = string.Empty;

        [ObservableProperty]
        private string dbPass = string.Empty;

        // Called by source-generator when IsVerifyEnabled changes
        partial void OnIsVerifyEnabledChanged(bool value)
        {
            (VerifyOtpCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username)) { ErrorMessage = "Vui lòng nhập tên đăng nhập."; return; }
            if (string.IsNullOrWhiteSpace(Password)) { ErrorMessage = "Vui lòng nhập mật khẩu."; return; }

            try
            {
                IsVerifyEnabled = true;

                // Check if using external database configuration
                bool isExternalLogin = !string.IsNullOrWhiteSpace(DbHost) || 
                                      !string.IsNullOrWhiteSpace(DbUser) || 
                                      !string.IsNullOrWhiteSpace(DbPass);

                LoginResponseDto? loginData;

                if (isExternalLogin)
                {
                    // Validate external DB fields
                    if (string.IsNullOrWhiteSpace(DbHost)) { ErrorMessage = "Vui lòng nhập database host."; return; }
                    if (string.IsNullOrWhiteSpace(DbUser)) { ErrorMessage = "Vui lòng nhập database user."; return; }
                    if (string.IsNullOrWhiteSpace(DbPass)) { ErrorMessage = "Vui lòng nhập database password."; return; }

                    // External login
                    var externalDto = new LoginExternalRequestDto
                    {
                        UserName = Username,
                        Password = Password,
                        DbHost = DbHost,
                        DbPort = string.IsNullOrWhiteSpace(DbPort) ? "5432" : DbPort,
                        DbName = string.IsNullOrWhiteSpace(DbName) ? "postgres" : DbName,
                        DbUser = DbUser,
                        DbPass = DbPass
                    };

                    var result = await _authService.VerifyLoginExternalAsync(externalDto);
                    if (!result.Success || result.Data == null)
                    {
                        var errorMsg = result.Message ?? "Đăng nhập thất bại.";
                        if (result.Errors != null && result.Errors.Any())
                        {
                            errorMsg += "\n\nChi tiết lỗi:\n" + string.Join("\n", result.Errors);
                        }
                        ErrorMessage = errorMsg;
                        return;
                    }

                    loginData = result.Data;
                    _encryptedDbConfigInternal = loginData.EncryptedDbConfig ?? string.Empty;
                }
                else
                {
                    // Standard login - DB config is never persisted for security
                    var result = await _authService.VerifyLoginAsync(Username, Password);
                    if (!result.Success || result.Data == null)
                    {
                        var errorMsg = result.Message ?? "Đăng nhập thất bại.";
                        if (result.Errors != null && result.Errors.Any())
                        {
                            errorMsg += "\n\nChi tiết lỗi:\n" + string.Join("\n", result.Errors);
                        }
                        ErrorMessage = errorMsg;
                        return;
                    }

                    loginData = result.Data;
                    _encryptedDbConfigInternal = string.Empty;
                }

                _otpSessionIdInternal = loginData.OtpSessionId ?? string.Empty;
                RequiresOtp = loginData.RequiresOtp;

                var name = loginData.UserFullName ?? string.Empty;
                var email = loginData.MaskedEmail ?? string.Empty;
                OtpInfo = string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email) ? string.Empty : $"Xin chào {name}. Đã gửi OTP tới {email}".Trim();

                _otpExpiresInInternal = loginData.OtpExpiresIn;
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
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Lỗi kết nối:\n{ex.Message}\n\nVui lòng kiểm tra:\n- Server có đang chạy không?\n- URL server có đúng không?";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi không xác định:\n{ex.Message}";
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

            var dto = new OtpResendRequestDto 
            { 
                OtpSessionId = _otpSessionIdInternal,
                EncryptedDbConfig = _encryptedDbConfigInternal
            };
            var result = await _authService.ResendOtpAsync(dto);
            
            if (!result.Success || result.Data == null)
            {
                var errorMsg = result.Message ?? "Không thể gửi lại OTP.";
                
                if (result.Errors != null && result.Errors.Any())
                {
                    errorMsg += "\n\nChi tiết lỗi:\n" + string.Join("\n", result.Errors);
                }
                
                ErrorMessage = errorMsg;
                return;
            }

            var resp = result.Data;
            if (!resp.IsOtpResent)
            {
                var errorMsg = result.Message ?? "Không thể gửi lại OTP.";
                
                if (result.Errors != null && result.Errors.Any())
                {
                    errorMsg += "\n\nChi tiết lỗi:\n" + string.Join("\n", result.Errors);
                }
                
                ErrorMessage = errorMsg;
                return;
            }

            if (!string.IsNullOrWhiteSpace(resp.NewOtpSessionId))
            {
                _otpSessionIdInternal = resp.NewOtpSessionId;
            }

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

            var dto = new OtpVerifyRequestDto 
            { 
                OtpSessionId = _otpSessionIdInternal, 
                OtpCode = otpCode,
                EncryptedDbConfig = _encryptedDbConfigInternal
            };
            var result = await _authService.VerifyOtpAsync(dto);
            
            if (!result.Success || result.Data == null)
            {
                var errorMsg = result.Message ?? "Mã OTP không hợp lệ.";
                
                if (result.Errors != null && result.Errors.Any())
                {
                    errorMsg += "\n\nChi tiết lỗi:\n" + string.Join("\n", result.Errors);
                }
                
                ErrorMessage = errorMsg;
                return false;
            }

            var otpData = result.Data;
            TokenStore.AccessToken = otpData.AccessToken;
            TokenStore.RefreshToken = otpData.RefreshToken;

            try
            {
                if (!string.IsNullOrWhiteSpace(TokenStore.RefreshToken))
                {
                    // Save refresh token with encrypted DB config
                    // Use the config from OTP response if available, otherwise use the internal one from login
                    var dbConfigToSave = !string.IsNullOrWhiteSpace(otpData.EncryptedDbConfig) 
                        ? otpData.EncryptedDbConfig 
                        : _encryptedDbConfigInternal;
                    
                    // Clear DB config if this is a standard login (no encrypted config)
                    bool clearDbConfig = string.IsNullOrWhiteSpace(dbConfigToSave);
                    TokenPersistence.SaveRefreshToken(TokenStore.RefreshToken, dbConfigToSave, clearDbConfig);
                }
            }
            catch { }

            // DB credentials are NEVER saved for security reasons
            // User must re-enter external DB config on each login if needed

            Helpers.AppState.CurrentUser = new UserDto
            {
                UserId = 0,
                UserName = otpData.UserName,
                UserFullName = otpData.UserFullName,
                RoleId = otpData.RoleId,
                RoleName = otpData.RoleName,
                IsActive = true
            };

            Helpers.AppState.HasSeenGuide = otpData.HasSeenGuide;

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
