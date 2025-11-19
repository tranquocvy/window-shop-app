using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class ApiClientFactory
    {
        // Shared lazy HttpClient for the application
        public static readonly Lazy<HttpClient> SharedClient = new(Create);

        private static HttpClient Create()
        {
            var client = new HttpClient
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
