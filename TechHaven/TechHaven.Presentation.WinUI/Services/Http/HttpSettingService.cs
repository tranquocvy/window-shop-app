using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using System.Diagnostics;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpSettingService : IAppSettingService
    {
        private readonly HttpClient _client;
        // Controller name is 'AppSettingController' -> route 'api/AppSetting'
        private const string BaseUrl = "api/AppSetting";

        public HttpSettingService(HttpClient client)
        {
            _client = client;
        }

        public async Task<AppSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            var resp = await _client.GetAsync($"{BaseUrl}/{key}", cancellationToken);
            Debug.WriteLine($"GET {BaseUrl}/{key} -> {(int)resp.StatusCode} {resp.ReasonPhrase}");

            if (!resp.IsSuccessStatusCode)
            {
                try
                {
                    var txt = await resp.Content.ReadAsStringAsync(cancellationToken);
                    Debug.WriteLine($"GET {BaseUrl}/{key} response body: {txt}");
                }
                catch { }

                return null;
            }

            try
            {
                // API returns ResponseWrapper<AppSettingDto>
                var wrapper = await resp.Content.ReadFromJsonAsync<ResponseWrapper<AppSettingDto>>(cancellationToken: cancellationToken);
                if (wrapper == null)
                {
                    Debug.WriteLine($"GET {BaseUrl}/{key} returned empty wrapper");
                    return null;
                }

                Debug.WriteLine($"GET {BaseUrl}/{key} wrapper Success={wrapper.Success} DataKey={wrapper.Data?.Key} DataValue={wrapper.Data?.Value}");
                return wrapper.Data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to deserialize GET {BaseUrl}/{key}: {ex}");
                return null;
            }
        }

        public async Task<bool> UpsertAsync(string key, AppSettingUpsertRequestDto dto, CancellationToken cancellationToken = default)
        {
            if (dto != null)
            {
                dto.Key = key;
            }

            // Check if the setting already exists. If not, use POST to create; otherwise use PUT to update.
            var existing = await GetByKeyAsync(key, cancellationToken);

            HttpResponseMessage resp;
            if (existing == null)
            {
                // Create
                resp = await _client.PostAsJsonAsync(BaseUrl, dto, cancellationToken);
                Debug.WriteLine($"POST {BaseUrl} -> {(int)resp.StatusCode} {resp.ReasonPhrase}");
            }
            else
            {
                // Update
                resp = await _client.PutAsJsonAsync($"{BaseUrl}/{key}", dto, cancellationToken);
                Debug.WriteLine($"PUT {BaseUrl}/{key} -> {(int)resp.StatusCode} {resp.ReasonPhrase}");
            }

            // Try to parse response wrapper to get Success flag and Data
            try
            {
                var wrapper = await resp.Content.ReadFromJsonAsync<ResponseWrapper<AppSettingDto>>(cancellationToken: cancellationToken);
                if (wrapper != null)
                {
                    Debug.WriteLine($"Upsert wrapper Success={wrapper.Success} DataKey={wrapper.Data?.Key} DataValue={wrapper.Data?.Value}");
                    return wrapper.Success;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to deserialize upsert response wrapper: {ex}");
            }

            // Fallback to HTTP status
            if (!resp.IsSuccessStatusCode)
            {
                try
                {
                    var txt = await resp.Content.ReadAsStringAsync(cancellationToken);
                    Debug.WriteLine($"Upsert response body: {txt}");
                }
                catch { }
            }

            return resp.IsSuccessStatusCode;
        }
    }
}
