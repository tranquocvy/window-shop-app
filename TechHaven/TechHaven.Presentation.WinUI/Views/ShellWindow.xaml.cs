using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Themes;
using TechHaven.Presentation.WinUI.ViewModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.Threading.Tasks;
using Windows.System;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Onboarding;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class ShellWindow : Window
    {
        private AppWindow? _appWindow;
        private readonly SettingViewModel _settingViewModel;
        private readonly TrialModeManager _trialModeManager;
        private ChatBotManager? _chatBotManager;
        private readonly IUserService _userService = new HttpUserService(ApiClientFactory.GetHttpClient());
        private readonly IOnboardingService _onboardingService = new OnboardingService();

        public ShellWindow()
        {
            this.InitializeComponent();

            _settingViewModel = new SettingViewModel();
            _trialModeManager = new TrialModeManager(this, new Services.Http.HttpAuthService(ApiClientFactory.GetHttpClient()));

            // Extend content into title bar so we can use a custom title area
            try
            {
                var hWnd = WindowNative.GetWindowHandle(this);
                var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                if (appWindow is not null)
                {
                    _appWindow = appWindow;
                    appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                    // Set system title bar buttons (minimize/maximize/close) colors
                    try
                    {
                        var transparent = Colors.Transparent;
                        appWindow.TitleBar.ButtonBackgroundColor = transparent;
                        appWindow.TitleBar.ButtonHoverBackgroundColor = transparent;
                        appWindow.TitleBar.ButtonPressedBackgroundColor = transparent;
                        appWindow.TitleBar.ButtonInactiveBackgroundColor = transparent;

                        // Apply current theme color
                        ApplyTitleBarColors();

                        // Listen for theme changes so titlebar buttons update immediately
                        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged_ForTitlebar;

                        // Unsubscribe when window closes to avoid memory leaks and crashes on re-login
                        this.Closed += (s, e) =>
                        {
                            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged_ForTitlebar;
                        };
                    }
                    catch
                    {
                        // ignore failures applying titlebar colors
                    }
                }
            }
            catch
            {
                // ignore on platforms where Windowing APIs are not available
            }

            if (Content is FrameworkElement root)
            {
                ThemeManager.RegisterRoot(root);

                // Block ESC when trial mode is active
                try
                {
                    var escAccel = new KeyboardAccelerator { Key = VirtualKey.Escape };
                    escAccel.Invoked += (s, e) =>
                    {
                        if (_trialModeManager.IsTrialModeActive)
                            e.Handled = true;
                    };
                    root.KeyboardAccelerators.Add(escAccel);
                }
                catch { }
            }

            if (AppState.CurrentUser != null)
            {
                string userFullName = AppState.CurrentUser.UserFullName;
                string roleName = AppState.CurrentUser.RoleName;

                currentUserFullNameText.Text = userFullName;
                currentUserRoleText.Text = roleName;

            }

            // Initialize ChatBot
            InitializeChatBot();

            // Initialize settings and navigate to last visited page (or dashboard)
            _ = InitializeSettingsAndNavigateAsync();

            // Show onboarding if needed
            _ = ShowOnboardingIfNeededAsync();
        }

        public void NavigateTo(Type pageType)
        {
            try { contentFrame.Navigate(pageType); } catch { }
        }

        private async Task ShowOnboardingIfNeededAsync()
        {
            try
            {
                if (!AppState.HasSeenGuide)
                {
                    await _onboardingService.RunAsync(this);
                    // After run, update server flag
                    var resp = await _userService.UpdateGuideStatusAsync(true);
                    if (resp.Success && resp.Data)
                    {
                        AppState.HasSeenGuide = true;
                    }
                }
            }
            catch { }
        }

        private void InitializeChatBot()
        {
            try
            {
                _chatBotManager = new ChatBotManager(
                    ChatMinimizedButton,
                    ChatExpandedWindow,
                    ChatMessageInputBox,
                    ChatSendButton,
                    ChatCloseButton,
                    ChatMessagesPanel,
                    ChatMessageScrollViewer
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize ChatBot: {ex.Message}");
            }
        }

        private async Task InitializeSettingsAndNavigateAsync()
        {
            try
            {
                await _settingViewModel.InitializeAsync();

                // ensure navView is ready
                if (!navView.IsLoaded)
                {
                    // wait for Loaded event
                    var tcs = new TaskCompletionSource<bool>();
                    void handler(object s, RoutedEventArgs e) { navView.Loaded -= handler; tcs.SetResult(true); }
                    navView.Loaded += handler;
                    await tcs.Task;
                }

                var tag = _settingViewModel.LastVisitedPage;
                Type pageType = typeof(DashboardPage);

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    switch (tag)
                    {
                        case "dashboard": pageType = typeof(DashboardPage); break;
                        case "product": pageType = typeof(ProductPage); break;
                        case "order": pageType = typeof(OrderPage); break;
                        case "customer": pageType = typeof(CustomerPage); break;
                        case "report": pageType = typeof(ReportPage); break;
                        case "setting": pageType = typeof(SettingPage); break;
                        default: pageType = typeof(DashboardPage); break;
                    }

                    // try to select the nav item if available
                    var navItem = navView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => (x.Tag?.ToString()) == tag);
                    if (navItem != null) navView.SelectedItem = navItem;
                }

                // Navigate frame
                contentFrame.Navigate(pageType);

                // After navigation, check trial status
                _ = CheckTrialAsync();
            }
            catch
            {
                // fallback to dashboard
                try { contentFrame.Navigate(typeof(DashboardPage)); } catch { }
            }
        }

        public Task TriggerTrialCheckAsync()
        {
            return _trialModeManager.CheckAndShowTrialModeAsync();
        }

        private async Task CheckTrialAsync()
        {
            await _trialModeManager.CheckAndShowTrialModeAsync();
        }

        private void ThemeManager_ThemeChanged_ForTitlebar(ThemeManager.ThemeType obj)
        {
            // update titlebar colors on UI thread
            _ = this.DispatcherQueue?.TryEnqueue(() =>
            {
                ApplyTitleBarColors();
                // Also update nav toggle brush if nav already loaded
                try { UpdateNavToggleBrush(); } catch { }
            });
        }

        private void ApplyTitleBarColors()
        {
            try
            {
                if (_appWindow == null) return;

                var foreground = TryGetColorFromResource("TH.TextPrimary", Windows.UI.Color.FromArgb(255, 255, 255, 255));
                _appWindow.TitleBar.ButtonForegroundColor = foreground;
                _appWindow.TitleBar.ButtonHoverForegroundColor = foreground;
                _appWindow.TitleBar.ButtonPressedForegroundColor = foreground;
                _appWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb((byte)Math.Min(255, (int)(foreground.A * 0.7)), foreground.R, foreground.G, foreground.B);
            }
            catch { }
        }

        private void navView_Loaded(object sender, RoutedEventArgs e)
        {
            // only update visual aspects here; actual navigation handled by InitializeSettingsAndNavigateAsync
            try
            {
                UpdateNavToggleBrush();
            }
            catch
            {
                // ignore failures during visual tree manipulations
            }
        }

        private void UpdateNavToggleBrush()
        {
            try
            {
                // Ensure NavigationView itself uses theme resource (set in XAML)
                // Find the toggle button in the nav view template
                string[] possibleNames = new[] { "TogglePaneButton", "PaneToggleButton", "TogglePaneToggleButton" };

                Button? toggleButton = null;
                foreach (var name in possibleNames)
                {
                    toggleButton = FindDescendant<Button>(navView, name);
                    if (toggleButton != null) break;
                }

                if (toggleButton == null)
                {
                    toggleButton = FindDescendant<Button>(navView);
                }

                if (toggleButton != null)
                {
                    try { toggleButton.ApplyTemplate(); } catch { }

                    try
                    {
                        ClearVisualStateStoryboardsRecursively(toggleButton);
                    }
                    catch { }

                    try
                    {
                        toggleButton.ClearValue(Control.ForegroundProperty);
                        toggleButton.ClearValue(Control.BackgroundProperty);

                        var fontIcon = FindDescendant<FontIcon>(toggleButton);
                        var symbolIcon = FindDescendant<SymbolIcon>(toggleButton);
                        var pathIcon = FindDescendant<PathIcon>(toggleButton);

                        if (fontIcon != null) fontIcon.ClearValue(IconElement.ForegroundProperty);
                        if (symbolIcon != null) symbolIcon.ClearValue(IconElement.ForegroundProperty);
                        if (pathIcon != null) pathIcon.ClearValue(IconElement.ForegroundProperty);
                    }
                    catch { }
                }
            }
            catch { }
        }

        // Recursively clears Storyboard on VisualStates in the subtree
        private static void ClearVisualStateStoryboardsRecursively(DependencyObject node)
        {
            if (node == null) return;

            if (node is FrameworkElement fe)
            {
                try
                {
                    var groups = VisualStateManager.GetVisualStateGroups(fe);
                    if (groups != null)
                    {
                        foreach (var g in groups)
                        {
                            foreach (var s in g.States)
                            {
                                try { s.Storyboard = null; } catch { }
                            }
                        }
                    }
                }
                catch { }
            }

            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(node, i);
                ClearVisualStateStoryboardsRecursively(child);
            }
        }

        private async void navView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
                return;

            if (args.InvokedItemContainer == null)
                return;

            string? tag = args.InvokedItemContainer.Tag as string;
            if (tag == null) return;

            Type pageType;

            switch (tag)
            {
                case "dashboard":
                    pageType = typeof(DashboardPage);
                    break;
                case "product":
                    pageType = typeof(ProductPage);
                    break;
                case "order":
                    pageType = typeof(OrderPage);
                    break;
                case "customer":
                    pageType = typeof(CustomerPage);
                    break;
                case "report":
                    pageType = typeof(ReportPage);
                    break;
                case "setting":
                    pageType = typeof(SettingPage);
                    break;

                case "logout":
                    ContentDialog logoutDialog = new ContentDialog
                    {
                        Title = "Log Out ",
                        Content = "Do you want log out?",
                        PrimaryButtonText = "Log Out",
                        CloseButtonText = "Cancel"
                    };

                    logoutDialog.XamlRoot = this.Content.XamlRoot; 

                    ContentDialogResult result = await logoutDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        AppState.CurrentUser = null;

                        // Remove persisted refresh token and clear in-memory tokens
                        try
                        {
                            TokenPersistence.RemoveRefreshToken();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Logout: failed to remove persisted token: {ex.Message}");
                        }

                        // clear in-memory tokens
                        try { TokenStore.RefreshToken = null; } catch { }
                        try { TokenStore.AccessToken = null; } catch { }

                        var loginWindow = new MainWindow();
                        App.MainWindow = loginWindow;
                        loginWindow.Activate();

                        this.Close();
                    }

                    return;

                default:
                    pageType = typeof(DashboardPage);
                    break;
            }

            contentFrame.Navigate(pageType);

            // Persist last visited (ignore logout)
            try
            {
                if (tag != "logout")
                {
                    await _settingViewModel.SetLastVisitedPageAsync(tag);
                }
            }
            catch { }
        }

        // Recursive helper to find descendant in Visual Tree
        private static T? FindDescendant<T>(DependencyObject parent, string? name = null) where T : DependencyObject
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    if (string.IsNullOrEmpty(name)) return t;
                    if (child is FrameworkElement fe && fe.Name == name) return t;
                }

                var result = FindDescendant<T>(child, name);
                if (result != null) return result;
            }

            return null;
        }

        private static object? FindResourceInMergedDictionaries(string key)
        {
            try
            {
                if (Application.Current?.Resources != null)
                {
                    // check root resources first
                    if (Application.Current.Resources.ContainsKey(key))
                        return Application.Current.Resources[key];

                    // search merged dictionaries recursively
                    foreach (var md in Application.Current.Resources.MergedDictionaries)
                    {
                        var found = FindInDictionaryRecursive(md, key);
                        if (found != null) return found;
                    }
                }
            }
            catch { }

            return null;
        }

        private static object? FindInDictionaryRecursive(ResourceDictionary dict, string key)
        {
            try
            {
                if (dict == null) return null;
                if (dict.ContainsKey(key)) return dict[key];

                foreach (var md in dict.MergedDictionaries)
                {
                    var f = FindInDictionaryRecursive(md, key);
                    if (f != null) return f;
                }
            }
            catch { }

            return null;
        }

        private static Windows.UI.Color TryGetColorFromResource(string key, Windows.UI.Color fallback)
        {
            try
            {
                // Check the application resources first
                if (Application.Current?.Resources != null && Application.Current.Resources.ContainsKey(key))
                {
                    var res = Application.Current.Resources[key];
                    if (res is SolidColorBrush scb)
                        return scb.Color;
                    if (res is Windows.UI.Color c)
                        return c;
                }

                // Search merged dictionaries recursively
                var found = FindResourceInMergedDictionaries(key);
                if (found != null)
                {
                    if (found is SolidColorBrush scb2)
                        return scb2.Color;
                    if (found is Windows.UI.Color c2)
                        return c2;
                }
            }
            catch { }

            return fallback;
        }

        public FrameworkElement? GetCurrentPageRoot()
        {
            try { return contentFrame.Content as FrameworkElement; } catch { return null; }
        }
    }
}
