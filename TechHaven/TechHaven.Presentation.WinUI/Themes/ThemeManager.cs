using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TechHaven.Presentation.WinUI;

namespace TechHaven.Presentation.WinUI.Themes
{
    internal static class ThemeManager
    {
        public enum ThemeType
        {
            Light,
            Dark,
            HyperViolet,
            Midnight
        }

        public static ThemeType CurrentTheme { get; private set; } = ThemeType.Light;

        private const string LightPath = "ms-appx:///Themes/LightTheme.xaml";
        private const string DarkPath = "ms-appx:///Themes/DarkTheme.xaml";
        private const string HyperVioletPath = "ms-appx:///Themes/HyperVioletTheme.xaml";
        private const string MidnightPath = "ms-appx:///Themes/MidnightTheme.xaml";
        private const string AccentsPath = "ms-appx:///Themes/Accents.xaml";

        private static readonly List<WeakReference<FrameworkElement>> _registeredRoots = new();

        public static event Action<ThemeType>? ThemeChanged;

        public static void Initialize(ThemeType defaultTheme = ThemeType.Light, bool loadAccents = false)
        {
            ApplyTheme(defaultTheme);
            if (loadAccents)
                ApplyAccents();
        }

        public static void ApplyTheme(ThemeType theme)
        {
            var app = Application.Current;
            if (app == null) return;

            RemoveThemeDictionaries(app);

            string path = theme switch
            {
                ThemeType.Light => LightPath,
                ThemeType.Dark => DarkPath,
                ThemeType.HyperViolet => HyperVioletPath,
                ThemeType.Midnight => MidnightPath,
                _ => LightPath
            };

            try
            {
                var dict = new ResourceDictionary { Source = new Uri(path) };
                app.Resources.MergedDictionaries.Add(dict);
                CurrentTheme = theme;

                ReapplyAll();
                ThemeChanged?.Invoke(theme);
            }
            catch (Exception)
            {
                // ignore load errors
            }
        }

        public static void ToggleTheme()
        {
            var values = Enum.GetValues(typeof(ThemeType)).Cast<ThemeType>().ToArray();
            int idx = Array.IndexOf(values, CurrentTheme);
            int next = (idx + 1) % values.Length;
            ApplyTheme(values[next]);
        }

        public static void ApplyAccents()
        {
            var app = Application.Current;
            if (app == null) return;

            var existing = app.Resources.MergedDictionaries.FirstOrDefault(d => 
                d.Source != null && d.Source.OriginalString.Contains("Accents.xaml", StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                app.Resources.MergedDictionaries.Remove(existing);

            try
            {
                var accentDict = new ResourceDictionary { Source = new Uri(AccentsPath) };
                app.Resources.MergedDictionaries.Add(accentDict);

                ReapplyAll();
                ThemeChanged?.Invoke(CurrentTheme);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static void RemoveThemeDictionaries(Application app)
        {
            var toRemove = app.Resources.MergedDictionaries
                .Where(d => d.Source != null && 
                    d.Source.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var d in toRemove)
                app.Resources.MergedDictionaries.Remove(d);
        }

        public static void ApplyTo(FrameworkElement root)
        {
            if (root == null) return;
            var appRes = Application.Current?.Resources;
            if (appRes == null) return;

            ApplyBackgroundBrush(root, appRes);
            ApplyNavigationViewTheme(root, appRes);
            ApplyBordersTheme(root, appRes);
            ApplyTextBlocksTheme(root, appRes);
            ApplyChatBotTheme(root, appRes);
        }

        private static void ApplyBackgroundBrush(FrameworkElement root, ResourceDictionary appRes)
        {
            try
            {
                if (appRes.ContainsKey("TH.SurfaceBackground"))
                {
                    var brush = appRes["TH.SurfaceBackground"] as Brush;
                    if (brush != null)
                    {
                        if (root is Panel panel)
                            panel.Background = brush;
                        else if (root is Control control)
                            control.Background = brush;
                        else
                        {
                            var prop = root.GetType().GetProperty("Background");
                            if (prop != null && prop.CanWrite && prop.PropertyType.IsAssignableFrom(typeof(Brush)))
                                prop.SetValue(root, brush);
                        }
                    }
                }
            }
            catch { }
        }

        private static void ApplyNavigationViewTheme(FrameworkElement root, ResourceDictionary appRes)
        {
            try
            {
                var nav = FindElementByName<NavigationView>(root, "navView");
                if (nav != null)
                {
                    if (appRes.ContainsKey("TH.NavBackground"))
                    {
                        var nb = appRes["TH.NavBackground"] as Brush;
                        if (nb != null) nav.Background = nb;
                    }

                    if (appRes.ContainsKey("TH.TextPrimary"))
                    {
                        var tp = appRes["TH.TextPrimary"] as Brush;
                        if (tp != null) nav.Foreground = tp;
                    }

                    if (appRes.ContainsKey("TH.TextSecondary"))
                    {
                        var textSecondary = appRes["TH.TextSecondary"] as Brush;
                        if (textSecondary != null && nav.MenuItems != null)
                        {
                            foreach (var item in nav.MenuItems)
                            {
                                if (item is NavigationViewItem navItem)
                                {
                                    navItem.Foreground = textSecondary;

                                    if (navItem.Icon is IconElement icon)
                                    {
                                        icon.Foreground = textSecondary;
                                    }
                                }
                            }
                        }
                    }

                    nav.UpdateLayout();
                }
            }
            catch { }
        }

        private static void ApplyBordersTheme(FrameworkElement root, ResourceDictionary appRes)
        {
            try
            {
                var borders = FindAllDescendants<Border>(root);
                foreach (var border in borders)
                {
                    if (border.Name?.Contains("Card", StringComparison.OrdinalIgnoreCase) == true ||
                        border.Name?.Contains("addUserBorder", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (appRes.ContainsKey("TH.CardBackground"))
                        {
                            var bg = appRes["TH.CardBackground"] as Brush;
                            if (bg != null) border.Background = bg;
                        }
                    }

                    if (border.BorderThickness.Top > 0 && appRes.ContainsKey("TH.BorderBrush"))
                    {
                        var borderBrush = appRes["TH.BorderBrush"] as Brush;
                        if (borderBrush != null) border.BorderBrush = borderBrush;
                    }
                }
            }
            catch { }
        }

        private static void ApplyTextBlocksTheme(FrameworkElement root, ResourceDictionary appRes)
        {
            try
            {
                var fullName = FindElementByName<TextBlock>(root, "currentUserFullNameText");
                if (fullName != null && appRes.ContainsKey("TH.TextPrimary"))
                {
                    var tp = appRes["TH.TextPrimary"] as Brush;
                    if (tp != null) fullName.Foreground = tp;
                }

                var role = FindElementByName<TextBlock>(root, "currentUserRoleText");
                if (role != null && appRes.ContainsKey("TH.TextSecondary"))
                {
                    var ts = appRes["TH.TextSecondary"] as Brush;
                    if (ts != null) role.Foreground = ts;
                }

                var titleText = FindElementByName<TextBlock>(root, "titleText");
                if (titleText != null && appRes.ContainsKey("TH.TextPrimary"))
                {
                    var tp = appRes["TH.TextPrimary"] as Brush;
                    if (tp != null) titleText.Foreground = tp;
                }
            }
            catch { }
        }

        private static void ApplyChatBotTheme(FrameworkElement root, ResourceDictionary appRes)
        {
            try
            {
                var chatWindow = FindElementByName<Grid>(root, "ChatExpandedWindow");
                if (chatWindow != null && appRes.ContainsKey("TH.NavBackground"))
                {
                    var bg = appRes["TH.NavBackground"] as Brush;
                    if (bg != null) chatWindow.Background = bg;
                }
            }
            catch { }
        }

        private static T? FindElementByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            try
            {
                var element = parent as FrameworkElement;
                if (element != null)
                {
                    var found = element.FindName(name);
                    if (found is T t) return t;
                }
            }
            catch { }

            return FindDescendantByName<T>(parent, name);
        }

        private static T? FindDescendantByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t && (child as FrameworkElement)?.Name == name)
                    return t;

                var result = FindDescendantByName<T>(child, name);
                if (result != null) return result;
            }

            return null;
        }

        private static List<T> FindAllDescendants<T>(DependencyObject parent) where T : DependencyObject
        {
            var results = new List<T>();
            if (parent == null) return results;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    results.Add(t);

                results.AddRange(FindAllDescendants<T>(child));
            }

            return results;
        }

        public static void RegisterRoot(FrameworkElement root)
        {
            if (root == null) return;
            lock (_registeredRoots)
            {
                if (!_registeredRoots.Any(wr => wr.TryGetTarget(out var t) && t == root))
                    _registeredRoots.Add(new WeakReference<FrameworkElement>(root));
            }

            ApplyTo(root);
        }

        private static void ReapplyAll()
        {
            lock (_registeredRoots)
            {
                for (int i = _registeredRoots.Count - 1; i >= 0; i--)
                {
                    if (_registeredRoots[i].TryGetTarget(out var root) && root != null)
                    {
                        ApplyTo(root);
                    }
                    else
                    {
                        _registeredRoots.RemoveAt(i);
                    }
                }
            }

            try
            {
                var main = App.MainWindow;
                if (main != null && main.Content is FrameworkElement mainRoot)
                {
                    ApplyTo(mainRoot);
                }
            }
            catch { }
        }
    }
}
