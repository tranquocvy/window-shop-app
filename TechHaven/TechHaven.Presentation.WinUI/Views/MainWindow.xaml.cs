using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            _viewModel = new LoginViewModel();
        }

        // Phương thức xử lý sự kiện Click
        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            // Set username and password from UI to ViewModel
            _viewModel.Username = usernameBox.Text;
            _viewModel.Password = passwordBox.Password;

            // Execute login command
            await _viewModel.LoginCommand.ExecuteAsync(null);

            // Check for errors
            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                errorTextBlock.Text = _viewModel.ErrorMessage;
                return;
            }

            // Check if OTP is required
            if (_viewModel.RequiresOtp)
            {
                var otpOk = await ShowOtpDialogAndVerifyAsync();
                if (!otpOk)
                {
                    errorTextBlock.Text = _viewModel.ErrorMessage;
                    return;
                }
            }

            // Complete login and get user info
            var success = await _viewModel.CompleteLoginAndGetUserAsync();
            if (!success)
            {
                errorTextBlock.Text = _viewModel.ErrorMessage;
                return;
            }

            // Navigate to ShellWindow
            var shellWindow = new ShellWindow();
            shellWindow.Activate();

            // Close login window
            this.Close();
        }

        private async Task<bool> ShowOtpDialogAndVerifyAsync()
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
                var ok = await _viewModel.VerifyOtpAsync(otpBox.Text?.Trim() ?? string.Empty);
                return ok;
            }

            return false;
        }
    }
}
