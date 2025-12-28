using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class HttpResponseExtensions
    {
        // Pattern for GET returning ResponseWrapper<T>
        public static async Task<ResponseWrapper<T>> GetWrapperFromJsonAsync<T>(this HttpClient httpClient, string url, string failureMessage = "Failed to retrieve data")
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<ResponseWrapper<T>>(url);
                return response ?? new ResponseWrapper<T> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<T>
                {
                    Success = false,
                    Message = failureMessage,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Pattern for POST/PUT/DELETE: read ResponseWrapper<T> even when API returns non-success status
        public static async Task<ResponseWrapper<T>> EnsureSuccessAndReadWrapperAsync<T>(this HttpResponseMessage response, string failureMessage = "Operation failed")
        {
            try
            {
                var result = await response.Content.ReadFromJsonAsync<ResponseWrapper<T>>();

                if (response.IsSuccessStatusCode)
                {
                    return result ?? new ResponseWrapper<T>
                    {
                        Success = false,
                        Message = failureMessage
                    };
                }

                if (result != null)
                {
                    result.Success = false;
                    if (string.IsNullOrWhiteSpace(result.Message))
                    {
                        result.Message = failureMessage;
                    }

                    result.Errors ??= new List<string>();
                    result.Errors.Add($"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                    return result;
                }

                return new ResponseWrapper<T>
                {
                    Success = false,
                    Message = failureMessage,
                    Errors = new List<string> { $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}" }
                };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<T>
                {
                    Success = false,
                    Message = failureMessage,
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
