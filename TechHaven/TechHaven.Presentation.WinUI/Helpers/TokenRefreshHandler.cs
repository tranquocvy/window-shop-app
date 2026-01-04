using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Helpers
{
    // DelegatingHandler that intercepts 401 responses, attempts refresh, and retries the request once.
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly string _baseAddress;

        public TokenRefreshHandler(string baseAddress)
        {
            _baseAddress = baseAddress?.TrimEnd('/') ?? string.Empty;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Attach access token if available
            if (!string.IsNullOrWhiteSpace(TokenStore.AccessToken) && request.Headers.Authorization == null)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.AccessToken);
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            // attempt refresh
            if (string.IsNullOrWhiteSpace(TokenStore.RefreshToken))
                return response;

            var refreshDto = new RefreshTokenRequestDto { RefreshToken = TokenStore.RefreshToken };

            try
            {
                using var client = new HttpClient { BaseAddress = new Uri(_baseAddress) };
                var refreshResp = await client.PostAsJsonAsync("api/Auth/refresh-token", refreshDto, cancellationToken).ConfigureAwait(false);
                if (!refreshResp.IsSuccessStatusCode)
                    return response;

                var wrapper = await refreshResp.Content.ReadFromJsonAsync<ResponseWrapper<RefreshTokenResponseDto>>(cancellationToken: cancellationToken).ConfigureAwait(false);
                if (wrapper == null || !wrapper.Success || wrapper.Data == null)
                    return response;

                // update tokens
                TokenStore.AccessToken = wrapper.Data.AccessToken;
                TokenStore.RefreshToken = wrapper.Data.NewRefreshToken;

                // retry original request with new access token
                var retry = await CloneHttpRequestMessageAsync(request).ConfigureAwait(false);
                retry.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.AccessToken);
                return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                return response;
            }
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            // copy the request's content (if any) into the cloned object
            if (req.Content != null)
            {
                var ms = new System.IO.MemoryStream();
                await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                // copy the content headers
                if (req.Content.Headers != null)
                {
                    foreach (var h in req.Content.Headers)
                        clone.Content.Headers.Add(h.Key, h.Value);
                }
            }

            foreach (var header in req.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            clone.Version = req.Version;
            return clone;
        }
    }
}
