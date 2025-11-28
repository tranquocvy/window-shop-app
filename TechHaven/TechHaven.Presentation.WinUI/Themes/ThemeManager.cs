using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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

        // Registered roots to update when theme changes
        private static readonly List<WeakReference<FrameworkElement>> _registeredRoots = new();

        // Initialize just loads the requested dictionaries. No automatic re-application.
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

            string path;
            switch (theme)
            {
                case ThemeType.Light:
                    path = LightPath;
                    break;
                case ThemeType.Dark:
                    path = DarkPath;
                    break;
                case ThemeType.HyperViolet:
                    path = HyperVioletPath;
                    break;
                case ThemeType.Midnight:
                    path = MidnightPath;
                    break;
                default:
                    path = LightPath;
                    break;
            }

            try
            {
                var dict = new ResourceDictionary { Source = new Uri(path) };
                app.Resources.MergedDictionaries.Add(dict);
                CurrentTheme = theme;

                // Re-apply to registered roots
                ReapplyAll();
            }
            catch (Exception)
            {
                // ignore load errors
            }
        }

        public static void ToggleTheme()
        {
            // Cycle through available themes in enum order
            var values = Enum.GetValues(typeof(ThemeType)).Cast<ThemeType>().ToArray();
            int idx = Array.IndexOf(values, CurrentTheme);
            int next = (idx + 1) % values.Length;
            ApplyTheme(values[next]);
        }

        public static void ApplyAccents()
        {
            var app = Application.Current;
            if (app == null) return;

            var existing = app.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Accents.xaml", StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                app.Resources.MergedDictionaries.Remove(existing);

            try
            {
                var accentDict = new ResourceDictionary { Source = new Uri(AccentsPath) };
                app.Resources.MergedDictionaries.Add(accentDict);

                // Re-apply to registered roots
                ReapplyAll();
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static void RemoveThemeDictionaries(Application app)
        {
            var toRemove = app.Resources.MergedDictionaries
                .Where(d => d.Source != null && (
                    d.Source.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var d in toRemove)
                app.Resources.MergedDictionaries.Remove(d);
        }

        // Apply theme brushes to a specific root element on demand
        public static void ApplyTo(FrameworkElement root)
        {
            if (root == null) return;
            var appRes = Application.Current?.Resources;
            if (appRes == null) return;

            // Use the TH.* keys used in your XAML
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

            // If there's a NavigationView named 'navView' under this root, apply nav brushes
            try
            {
                var nav = root.FindName("navView") as NavigationView;
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
                }
            }
            catch
            {
                // ignore find/assign errors
            }

            // Also apply Text brushes to known named textblocks if present
            try
            {
                var fullName = root.FindName("currentUserFullNameText") as TextBlock;
                if (fullName != null && appRes.ContainsKey("TH.TextPrimary"))
                {
                    var tp = appRes["TH.TextPrimary"] as Brush;
                    if (tp != null) fullName.Foreground = tp;
                }

                var role = root.FindName("currentUserRoleText") as TextBlock;
                if (role != null && appRes.ContainsKey("TH.TextSecondary"))
                {
                    var ts = appRes["TH.TextSecondary"] as Brush;
                    if (ts != null) role.Foreground = ts;
                }
            }
            catch
            {
                // ignore
            }
        }

        public static void RegisterRoot(FrameworkElement root)
        {
            if (root == null) return;
            lock (_registeredRoots)
            {
                // avoid duplicates
                if (!_registeredRoots.Any(wr => wr.TryGetTarget(out var t) && t == root))
                    _registeredRoots.Add(new WeakReference<FrameworkElement>(root));
            }

            // apply immediately
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
        }
    }
}
