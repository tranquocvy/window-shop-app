using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Themes;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class SettingPage : Page
    {
        private readonly SettingViewModel _viewModel;
        private bool _isInitializing = false;

        public SettingPage()
        {
            this.InitializeComponent();

            _viewModel = new SettingViewModel();

            LoadCurrentUser();
            UpdateThemeStatus();

            pageSizeCombo.ItemsSource = new int[] { 5, 10, 20, 50 };

            _ = InitializeViewModelAsync();
        }

        private void LoadCurrentUser()
        {
            var user = AppState.CurrentUser;
            if (user != null)
            {
                currentUserFullNameText.Text = string.IsNullOrWhiteSpace(user.UserFullName) ? user.UserName : user.UserFullName;
                currentUserRoleText.Text = user.RoleName ?? "-";
                currentUserUserNameText.Text = user.UserName ?? string.Empty;
                currentUserEmailText.Text = user.Email ?? string.Empty;
            }
            else
            {
                currentUserFullNameText.Text = "Not signed in";
                currentUserRoleText.Text = "-";
                currentUserUserNameText.Text = string.Empty;
                currentUserEmailText.Text = string.Empty;
            }

            // Only show add-user section for admins
            try
            {
                if (AppState.CurrentUser != null && AppState.CurrentUser.RoleId == 1)
                    addUserBorder.Visibility = Visibility.Visible;
                else
                    addUserBorder.Visibility = Visibility.Collapsed;
            }
            catch { }
        }

        private void UpdateThemeStatus()
        {
            var themeText = _viewModel.CurrentTheme ?? ThemeManager.CurrentTheme.ToString();
            themeStatusText.Text = $"Current: {themeText}";
            themeButton.Content = $"Theme: {themeText}";
        }

        private async void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new MenuFlyout();

            foreach (ThemeManager.ThemeType t in Enum.GetValues(typeof(ThemeManager.ThemeType)))
            {
                var item = new MenuFlyoutItem { Text = t.ToString() };
                var theme = t;
                item.Click += async (_, _) =>
                {
                    var ok = await _viewModel.SetThemeAsync(theme.ToString());
                    if (ok)
                    {
                        ThemeManager.ApplyTheme(theme);
                        ReloadPage();
                    }
                    else
                    {
                        var dlg = new ContentDialog
                        {
                            Title = "Save failed",
                            Content = "Unable to save theme setting.",
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await dlg.ShowAsync();
                    }
                };
                flyout.Items.Add(item);
            }

            flyout.ShowAt(themeButton);
        }

        private void ReloadPage()
        {
            try
            {
                var frame = this.Frame;
                if (frame != null)
                {
                    frame.Navigate(typeof(SettingPage));
                }
            }
            catch
            {
                UpdateThemeStatus();
            }
        }

        private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (pageSizeCombo.SelectedItem is int selected)
            {
                var ok = await _viewModel.SetPageSizeAsync(selected);
                if (!ok)
                {
                    var dlg = new ContentDialog
                    {
                        Title = "Save failed",
                        Content = "Unable to save page size setting.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dlg.ShowAsync();
                    pageSizeCombo.SelectedItem = _viewModel.PageSize;
                }
            }
        }

        private async Task InitializeViewModelAsync()
        {
            _isInitializing = true;
            try
            {
                try
                {
                    await _viewModel.InitializeAsync();
                }
                catch { }

                try
                {
                    UpdateThemeStatus();
                }
                catch { }

                try
                {
                    pageSizeCombo.SelectedItem = _viewModel.PageSize;
                }
                catch { }
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private (bool ok, string message) EvaluatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (false, "Password is required.");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters.");

            bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
            foreach (var c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSpecial = true;
            }

            if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
            {
                return (false, "Password must contain uppercase, lowercase, digit and special character.");
            }

            return (true, string.Empty);
        }

        private void ClearInlineErrors()
        {
            newUserEmailErrorText.Visibility = Visibility.Collapsed;
            newUserUserNameErrorText.Visibility = Visibility.Collapsed;
            newUserPasswordErrorText.Visibility = Visibility.Collapsed;
            newUserConfirmErrorText.Visibility = Visibility.Collapsed;
        }

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInlineErrors();

            var fullName = newUserFullNameBox.Text?.Trim() ?? string.Empty;
            var email = newUserEmailBox.Text?.Trim() ?? string.Empty;
            var username = newUserUserNameBox.Text?.Trim() ?? string.Empty;
            var password = newUserPasswordBox.Password ?? string.Empty;
            var confirm = newUserConfirmPasswordBox.Password ?? string.Empty;

            int roleId = 1;
            try
            {
                if (newUserRoleCombo.SelectedItem is ComboBoxItem cbi && cbi.Tag != null)
                    roleId = int.Parse(cbi.Tag.ToString() ?? "1");
            }
            catch { roleId = 1; }

            bool hasError = false;

            if (!IsValidEmail(email))
            {
                newUserEmailErrorText.Text = "Invalid email format.";
                newUserEmailErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                newUserUserNameErrorText.Text = "Username must be at least 3 characters.";
                newUserUserNameErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            var pwdEval = EvaluatePassword(password);
            if (!pwdEval.ok)
            {
                newUserPasswordErrorText.Text = pwdEval.message;
                newUserPasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (password != confirm)
            {
                newUserConfirmErrorText.Text = "Passwords do not match.";
                newUserConfirmErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (hasError)
            {
                return;
            }

            var signupDto = new SignupRequestDto
            {
                UserFullName = fullName,
                Email = email,
                UserName = username,
                Password = password,
                RoleId = roleId
            };

            addUserButton.IsEnabled = false;
            addUserButton.Content = "Creating...";

            try
            {
                var authService = new HttpAuthService(ApiClientFactory.GetHttpClient());
                ResponseWrapper<SignupResponseDto>? wrapper = null;
                try
                {
                    wrapper = await authService.SignupAsync(signupDto);
                }
                catch (HttpRequestException hx)
                {
                    var dlgErr = new ContentDialog
                    {
                        Title = "Network error",
                        Content = $"Unable to reach server: {hx.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dlgErr.ShowAsync();
                    return;
                }

                if (wrapper != null && wrapper.Success)
                {
                    var done = new ContentDialog
                    {
                        Title = "Success",
                        Content = wrapper.Message ?? "User created.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await done.ShowAsync();

                    newUserFullNameBox.Text = string.Empty;
                    newUserEmailBox.Text = string.Empty;
                    newUserUserNameBox.Text = string.Empty;
                    newUserPasswordBox.Password = string.Empty;
                    newUserConfirmPasswordBox.Password = string.Empty;
                    newUserRoleCombo.SelectedIndex = 0;
                }
                else
                {
                    var message = wrapper?.Message ?? "Unable to create user.";
                    if (wrapper?.Errors != null && wrapper.Errors.Count > 0)
                    {
                        message += "\n" + string.Join("\n", wrapper.Errors);
                    }

                    var fail = new ContentDialog
                    {
                        Title = "Failed",
                        Content = message,
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await fail.ShowAsync();
                }
            }
            finally
            {
                addUserButton.IsEnabled = true;
                addUserButton.Content = "Add user";
            }
        }
    }
}
