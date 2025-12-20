using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class AppState
    {
        private const string DefaultApiBaseUrl = "http://localhost:5207/";

        public static UserDto? CurrentUser { get; set; }
        public static bool IsLoggedIn => CurrentUser != null;

        // Backing field for API base URI (kept for HttpClient default if needed)
        private static Uri _apiBaseUri = new(
            Environment.GetEnvironmentVariable("TECHHAVEN_API_BASEURL") ??
            DefaultApiBaseUrl);

        // Expose the current API base URI (may be default until user configures)
        public static Uri ApiBaseUri => _apiBaseUri;

        // Flag to indicate whether user explicitly configured the API URL via Settings
        // Default is false so login is blocked until user saves settings
        public static bool IsApiConfigured { get; private set; } = false;

        public static void SetApiBaseUri(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            // Ensure scheme present; if missing, assume http
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "http://" + url;
            }

            // Ensure trailing slash for HttpClient BaseAddress consistency
            if (!url.EndsWith("/")) url += "/";

            try
            {
                var newUri = new Uri(url);
                _apiBaseUri = newUri;
                IsApiConfigured = true;
            }
            catch
            {
            }
        }

        // Global PageSize (Per-page) setting
        private static int _pageSize = 10; // default changed to 10
        public static int PageSize => _pageSize;
        public static event Action<int>? PageSizeChanged;
        public static void SetPageSize(int size)
        {
            if (size <= 0) return;
            _pageSize = size;
            try { PageSizeChanged?.Invoke(size); } catch { }
        }
    }
}
