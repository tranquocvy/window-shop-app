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
                // Read content as string first to check if it's valid JSON
                var contentString = await response.Content.ReadAsStringAsync();

                // Check if content is empty
                if (string.IsNullOrWhiteSpace(contentString))
                {
                    return new ResponseWrapper<T>
                    {
                        Success = false,
                        Message = "Server returned empty response",
                        Errors = new List<string> { $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}" }
                    };
                }

                // Check if content is HTML (common error page response)
                if (contentString.TrimStart().StartsWith("<") || contentString.TrimStart().StartsWith("<!DOCTYPE"))
                {
                    return new ResponseWrapper<T>
                    {
                        Success = false,
                        Message = $"{failureMessage} - Server returned HTML error page",
                        Errors = new List<string> 
                        { 
                            $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                            "Server returned HTML instead of JSON. Please check API URL and endpoint."
                        }
                    };
                }

                // Try to parse as JSON
                ResponseWrapper<T>? result;
                try
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<ResponseWrapper<T>>(
                        contentString,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    return new ResponseWrapper<T>
                    {
                        Success = false,
                        Message = $"{failureMessage} - Invalid JSON response",
                        Errors = new List<string> 
                        { 
                            $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                            $"JSON Parse Error: {jsonEx.Message}",
                            $"Response preview: {(contentString.Length > 200 ? contentString.Substring(0, 200) + "..." : contentString)}"
                        }
                    };
                }

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
                    Errors = new List<string> { $"Exception: {ex.Message}", $"Stack: {ex.StackTrace}" }
                };
            }
        }
    }
}
