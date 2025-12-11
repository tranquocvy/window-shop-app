using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Net.Http.Json;
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

            // Register this page root so ThemeManager can apply brushes to named elements
            ThemeManager.RegisterRoot(this);

            _viewModel = new SettingViewModel();

            LoadCurrentUser();

            UpdateThemeStatus();

            ThemeManager.ThemeChanged += OnThemeChanged;
            this.Unloaded += SettingPage_Unloaded;

            pageSizeCombo.ItemsSource = new int[] { 5, 10, 20, 50 };

            // Do NOT set SelectedItem here — wait until viewmodel is initialized to avoid triggering change handler
            // try { pageSizeCombo.SelectedItem = _viewModel.PageSize; } catch { }

            _ = InitializeViewModelAsync();
        }

        private void SettingPage_Unloaded(object? sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            this.Unloaded -= SettingPage_Unloaded;
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
            catch
            {
                // ignore if element not present
            }
        }

        private void UpdateThemeStatus()
        {
            // Prefer the viewmodel value if present (it may be null if we didn't initialize here), otherwise use ThemeManager's current theme
            var themeText = _viewModel.CurrentTheme ?? ThemeManager.CurrentTheme.ToString();
            themeStatusText.Text = $"Current: {themeText}";
            themeButton.Content = $"Theme: {themeText}";

            // Also apply current theme brushes to this page immediately
            ApplyThemeBrushes();
        }

        private void ApplyThemeBrushes()
        {
            var appRes = Application.Current?.Resources;
            if (appRes == null) return;

            // Helper to safely get brush
            Brush? GetBrush(String key) => appRes.ContainsKey(key) ? appRes[key] as Brush : null;

            // Page background
            var pageBg = GetBrush("TH.SurfaceBackground");
            if (pageBg != null)
            {
                if (this.Content is Panel panel)
                {
                    panel.Background = pageBg;
                }
                else if (this.Content is Control control)
                {
                    control.Background = pageBg;
                }
                else
                {
                    // attempt via reflection to set Background property if available
                    try
                    {
                        var root = this.Content as FrameworkElement;
                        var prop = root?.GetType().GetProperty("Background");
                        if (prop != null && prop.CanWrite && prop.PropertyType.IsAssignableFrom(typeof(Brush)))
                        {
                            prop.SetValue(root, pageBg);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            // Text colors
            var primary = GetBrush("TH.TextPrimary");
            var secondary = GetBrush("TH.TextSecondary");

            if (primary != null)
                currentUserFullNameText.Foreground = primary;
            if (secondary != null)
                currentUserRoleText.Foreground = secondary;

            // Also update the username and email fields
            if (secondary != null)
            {
                currentUserUserNameText.Foreground = secondary;
                currentUserEmailText.Foreground = secondary;
            }

            // Update title and color mode label so they change immediately
            if (primary != null)
            {
                try { titleText.Foreground = primary; } catch { }
                try { colorModeLabel.Foreground = primary; } catch { }
            }

            // Button accent/foreground
            var primaryBrush = GetBrush("TH.PrimaryBrush");
            if (primaryBrush != null)
                themeButton.Background = primaryBrush;

            // Style the page size combo to match theme
            if (primary != null)
            {
                try { pageSizeCombo.Foreground = primary; } catch { }
            }

            // Status text
            if (secondary != null)
                themeStatusText.Foreground = secondary;
        }

        private async void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a menu list of available themes for the user to choose from
            var flyout = new MenuFlyout();

            foreach (ThemeManager.ThemeType t in Enum.GetValues(typeof(ThemeManager.ThemeType)))
            {
                var item = new MenuFlyoutItem { Text = t.ToString() };
                var theme = t; // capture
                item.Click += async (_, _) =>
                {
                    // Persist via ViewModel (which calls the setting service)
                    var ok = await _viewModel.SetThemeAsync(theme.ToString());
                    if (ok)
                    {
                        ThemeManager.ApplyTheme(theme);
                        ApplyThemeBrushes();
                        UpdateThemeStatus();
                    }
                    else
                    {
                        // show a simple dialog to inform failure
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

            // Show the flyout anchored to the button
            flyout.ShowAt(themeButton);
        }

        private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (pageSizeCombo.SelectedItem is int selected)
            {
                // persist via viewmodel
                var ok = await _viewModel.SetPageSizeAsync(selected);
                if (!ok)
                {
                    // show simple dialog on failure
                    var dlg = new ContentDialog
                    {
                        Title = "Save failed",
                        Content = "Unable to save page size setting.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };

                    await dlg.ShowAsync();

                    // revert selection to current value
                    pageSizeCombo.SelectedItem = _viewModel.PageSize;
                }
            }
        }

        private void OnThemeChanged(ThemeManager.ThemeType obj)
        {
            _ = this.DispatcherQueue?.TryEnqueue(() =>
            {
                UpdateThemeStatus();
                ApplyThemeBrushes();
            });
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
                catch
                {
                    // ignore init failures (keep UI responsive)
                }

                // Update UI with loaded values
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
                // simple regex for email validation
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

            // Collect values from UI
            var fullName = newUserFullNameBox.Text?.Trim() ?? string.Empty;
            var email = newUserEmailBox.Text?.Trim() ?? string.Empty;
            var username = newUserUserNameBox.Text?.Trim() ?? string.Empty;
            var password = newUserPasswordBox.Password ?? string.Empty;
            var confirm = newUserConfirmPasswordBox.Password ?? string.Empty;

            int roleId = 1; // default Admin
            try
            {
                if (newUserRoleCombo.SelectedItem is ComboBoxItem cbi && cbi.Tag != null)
                    roleId = int.Parse(cbi.Tag.ToString() ?? "1");
            }
            catch { roleId = 1; }

            bool hasError = false;

            // Email validation
            if (!IsValidEmail(email))
            {
                newUserEmailErrorText.Text = "Invalid email format.";
                newUserEmailErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            // Username validation
            if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            {
                newUserUserNameErrorText.Text = "Username must be at least 3 characters.";
                newUserUserNameErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            // Password validation
            var pwdEval = EvaluatePassword(password);
            if (!pwdEval.ok)
            {
                newUserPasswordErrorText.Text = pwdEval.message;
                newUserPasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            // Confirm password
            if (password != confirm)
            {
                newUserConfirmErrorText.Text = "Passwords do not match.";
                newUserConfirmErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (hasError)
            {
                return; // show inline errors only
            }

            // Build signup DTO
            var signupDto = new SignupRequestDto
            {
                UserFullName = fullName,
                Email = email,
                UserName = username,
                Password = password,
                RoleId = roleId
            };

            // Disable button while processing
            addUserButton.IsEnabled = false;
            addUserButton.Content = "Creating...";

            try
            {
                var authService = new TechHaven.Presentation.WinUI.Services.Http.HttpAuthService(ApiClientFactory.GetHttpClient());
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

                    // Clear inputs
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
