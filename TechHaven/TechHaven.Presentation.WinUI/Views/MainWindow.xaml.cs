using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Auth;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly MockAuthService _authService = new MockAuthService();

        public MainWindow()
        {
            this.InitializeComponent();
        }

        // Phương thức xử lý sự kiện Click
        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            errorTextBlock.Text = ""; // Xóa lỗi cũ

            // 1. Lấy dữ liệu từ UI
            string username = usernameBox.Text;
            string password = passwordBox.Password;

            // 2. Gọi MockAuthService để xác thực
            var loginResult = await _authService.VerifyLoginAsync(username, password);

            // 3. Xử lý kết quả
            if (loginResult == null)
            {
                // ĐĂNG NHẬP THẤT BẠI
                errorTextBlock.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return;
            }

            if (loginResult.RequiresOtp)
            {
                // Ask for OTP via dialog
                var otpOk = await ShowOtpDialogAndVerifyAsync(loginResult.UserId);
                if (!otpOk)
                {
                    errorTextBlock.Text = "OTP không đúng hoặc đã bị hủy.";
                    return;
                }
            }

            // ĐĂNG NHẬP THÀNH CÔNG: lấy UserDto và set AppState
            var userDto = await _authService.GetUserDtoAsync(loginResult.UserId);
            if (userDto == null)
            {
                errorTextBlock.Text = "Không tìm thấy thông tin người dùng.";
                return;
            }

            AppState.CurrentUser = userDto;

            // 2. Tạo và kích hoạt cửa sổ chính (ShellWindow)
            var shellWindow = new ShellWindow();
            shellWindow.Activate();

            // 3. Đóng cửa sổ đăng nhập (MainWindow) này lại
            this.Close();
        }

        private async Task<bool> ShowOtpDialogAndVerifyAsync(int userId)
        {
            var otpBox = new TextBox { PlaceholderText = "Enter OTP", MaxLength = 6, Width = 220 };

            var dialog = new ContentDialog
            {
                Title = "Xác thực OTP",
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock { Text = "Vui lòng nhập mã OTP đã gửi.", TextWrapping = TextWrapping.Wrap },
                        new TextBlock { Text = "", Height = 6 },
                        otpBox
                    }
                },
                PrimaryButtonText = "Verify",
                SecondaryButtonText = "Cancel",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var dto = new OtpVerifyRequestDto { UserId = userId, OtpCode = otpBox.Text?.Trim() ?? string.Empty };
                var ok = await _authService.VerifyOtpAsync(dto);
                return ok;
            }

            return false;
        }
    }
}
