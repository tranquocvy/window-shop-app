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
            var user = await _authService.VerifyLoginAsync(username, password);

            // 3. Xử lý kết quả
            if (user != null)
            {
                // ĐĂNG NHẬP THÀNH CÔNG
                // 1. Lưu thông tin người dùng vào biến toàn cục
                AppState.CurrentUser = user;

                // 2. Tạo và kích hoạt cửa sổ chính (ShellWindow)
                var shellWindow = new ShellWindow();
                shellWindow.Activate();

                // 3. Đóng cửa sổ đăng nhập (MainWindow) này lại
                this.Close();
            }
            else
            {
                // ĐĂNG NHẬP THẤT BẠI
                errorTextBlock.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
            }
        }
    }
}
