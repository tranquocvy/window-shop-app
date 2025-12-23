using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/Product";
        private const string UploadUrl = "api/Image/upload";

        public HttpProductService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<ResponseWrapper<ProductDto>> GetProductsByIdAsync(int id)
        {
            return _httpClient.GetWrapperFromJsonAsync<ProductDto>($"{BaseUrl}/{id}", "Failed to retrieve product");
        }

        public async Task<ResponseWrapper<ProductDto>> CreateProductsAsync(ProductUpsertRequest dto)
        {

            // Gửi POST với mapped dto
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);

            return await response.EnsureSuccessAndReadWrapperAsync<ProductDto>(
                "Failed to create product"
            );
        }


        public async Task<ResponseWrapper<ProductDto>> UpdateProductsAsync(int id, ProductUpsertRequest dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<ProductDto>("Failed to update product");
        }

        public async Task<ResponseWrapper<bool>> DeleteProductsAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");

            // If server returns 204 NoContent (or any success with empty body) treat as success.
            if (response.IsSuccessStatusCode)
            {
                // No content -> success with boolean true
                if (response.Content == null || response.Content.Headers.ContentLength == 0)
                {
                    return new ResponseWrapper<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Deleted"
                    };
                }

                // Otherwise attempt to read the JSON wrapper
                return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to delete product");
            }

            // Non-success -> try to read body for message or return generic failure
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                return new ResponseWrapper<bool>
                {
                    Success = false,
                    Data = false,
                    Message = string.IsNullOrWhiteSpace(body) ? $"Server returned {(int)response.StatusCode}" : body
                };
            }
            catch
            {
                return new ResponseWrapper<bool>
                {
                    Success = false,
                    Data = false,
                    Message = $"Server returned {(int)response.StatusCode}"
                };
            }
        }

        public async Task<ResponseWrapper<PagingResponse<ProductDto>>> QueryProductsAsync(ProductListQueryDto query)
        {
            // Build query string from CustomerListQueryDto
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                queryParams.Add($"SearchTerm={Uri.EscapeDataString(query.SearchTerm)}");

            if (query.IsDraft.HasValue)
                queryParams.Add($"IsDraft={query.IsDraft.Value}");

            if (query.FromPrice.HasValue)
                queryParams.Add($"FromPrice={query.FromPrice.Value}");

            if (query.ToPrice.HasValue)
                queryParams.Add($"ToPrice={query.ToPrice.Value}");

            if (!string.IsNullOrWhiteSpace(query.Brand))
                queryParams.Add($"Brand={Uri.EscapeDataString(query.Brand)}");

            if (query.Status.HasValue)
                queryParams.Add($"Status={(int)query.Status.Value}");

            if (query.PageNumber > 0)
                queryParams.Add($"PageNumber={query.PageNumber}");

            if (query.PageSize > 0)
                queryParams.Add($"PageSize={query.PageSize}");

            string queryString = string.Join("&", queryParams);

            var url = string.IsNullOrEmpty(queryString) ? BaseUrl : $"{BaseUrl}?{queryString}";
            return await _httpClient.GetWrapperFromJsonAsync<PagingResponse<ProductDto>>(url, "Failed to query products");
        }

        public async Task<ResponseWrapper<string>> UploadImageAsync(Stream stream, string fileName, string contentType)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent("Apple"), "folder");

                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(streamContent, "file", fileName);

                // Gửi lên server
                var response = await _httpClient.PostAsync("api/Image/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    // 1. Đọc chuỗi JSON trả về
                    string jsonString = await response.Content.ReadAsStringAsync();

                    // 2. Bóc tách JSON để lấy link ảnh
                    // Cấu trúc JSON: { "data": { "imageUrl": "..." } }
                    var jsonNode = JsonNode.Parse(jsonString);

                    // Lấy giá trị của imageUrl, chuyển thành string
                    string? imageUrl = jsonNode?["data"]?["imageUrl"]?.ToString();

                    // 3. Trả về kết quả
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        return new ResponseWrapper<string>
                        {
                            Success = true,
                            Data = imageUrl, // Lúc này Data chỉ còn là "https://..." sạch đẹp
                            Message = "Upload thành công"
                        };
                    }
                    else
                    {
                        return new ResponseWrapper<string>
                        {
                            Success = false,
                            Message = "Không tìm thấy link ảnh trong phản hồi từ server"
                        };
                    }
                }
                else
                {
                    return new ResponseWrapper<string>
                    {
                        Success = false,
                        Message = $"Lỗi Server: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<string>
                {
                    Success = false,
                    Message = $"Lỗi kết nối: {ex.Message}",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // File: Services/Http/HttpProductService.cs

        public async Task<ResponseWrapper<bool>> DeleteImageAsync(string imageUrl)
        {
            try
            {
                // Gọi API: api/Image/delete?imageUrl=...
                // Lưu ý: Cần EscapeDataString vì imageUrl chứa ký tự đặc biệt như "://", "/"
                string requestUrl = $"api/Image?imageUrl={Uri.EscapeDataString(imageUrl)}";

                var response = await _httpClient.DeleteAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    return new ResponseWrapper<bool>
                    {
                        Success = true,
                        Message = "Xóa ảnh cũ thành công",
                        Data = true
                    };
                }
                else
                {
                    return new ResponseWrapper<bool>
                    {
                        Success = false,
                        Message = $"Lỗi xóa ảnh: {response.StatusCode}",
                        Data = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<bool>
                {
                    Success = false,
                    Message = $"Lỗi kết nối xóa ảnh: {ex.Message}",
                    Data = false
                };
            }
        }

    }
}
