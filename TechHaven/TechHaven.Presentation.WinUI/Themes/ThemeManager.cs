using System;
using System.Linq;
using Microsoft.UI.Xaml;

namespace TechHaven.Presentation.WinUI.Themes
{
    internal static class ThemeManager
    {
        public enum ThemeType
        {
            Light,
            Dark
        }

        public static ThemeType CurrentTheme { get; private set; } = ThemeType.Light;

        // Updated paths to match actual filenames in the Themes folder
        private const string LightPath = "ms-appx:///Themes/Light.xaml";
        private const string DarkPath = "ms-appx:///Themes/Dark.xaml";
        private const string AccentsPath = "ms-appx:///Themes/Accents.xaml";

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

            var path = theme == ThemeType.Light ? LightPath : DarkPath;
            try
            {
                var dict = new ResourceDictionary { Source = new Uri(path) };
                app.Resources.MergedDictionaries.Add(dict);
                CurrentTheme = theme;
            }
            catch (Exception)
            {
                // ignore load errors (file missing or invalid)
            }
        }

        public static void ToggleTheme()
        {
            ApplyTheme(CurrentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light);
        }

        public static void ApplyAccents()
        {
            var app = Application.Current;
            if (app == null) return;

            // remove existing accents if any
            var existing = app.Resources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("Accents.xaml", StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                app.Resources.MergedDictionaries.Remove(existing);

            try
            {
                var accentDict = new ResourceDictionary { Source = new Uri(AccentsPath) };
                app.Resources.MergedDictionaries.Add(accentDict);
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
                    d.Source.OriginalString.Contains("Light.xaml", StringComparison.OrdinalIgnoreCase) ||
                    d.Source.OriginalString.Contains("Dark.xaml", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var d in toRemove)
                app.Resources.MergedDictionaries.Remove(d);
        }
    }
}
