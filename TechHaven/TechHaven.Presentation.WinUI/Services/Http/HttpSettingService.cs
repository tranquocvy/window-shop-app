using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Presentation.WinUI.Services.Interfaces;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpSettingService : IAppSettingService
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "api/Setting";

        public HttpSettingService(HttpClient client)
        {
            _client = client;
        }

        public async Task<AppSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            var resp = await _client.GetAsync($"{BaseUrl}/{key}", cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<AppSettingDto>(cancellationToken: cancellationToken);
        }

        public async Task<bool> UpsertAsync(string key, AppSettingUpsertRequestDto dto, CancellationToken cancellationToken = default)
        {
            var resp = await _client.PostAsJsonAsync($"{BaseUrl}/{key}", dto, cancellationToken);
            return resp.IsSuccessStatusCode;
        }
    }
}
