using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class TokenStore
    {
        // In-memory token store for runtime (use secure storage in production)
        public static string? AccessToken { get; set; }
        public static string? RefreshToken { get; set; }
    }

    public static class ApiClientFactory
    {
        // Shared lazy HttpClient for the application
        public static readonly Lazy<HttpClient> SharedClient = new(Create);

        private static HttpClient Create()
        {
            // TokenRefreshHandler will attempt refresh when 401 is returned
            var handler = new TokenRefreshHandler(AppState.ApiBaseUri.ToString());
            var client = new HttpClient(handler)
            {
                BaseAddress = AppState.ApiBaseUri,
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public static HttpClient GetHttpClient() => SharedClient.Value;
    }
}
