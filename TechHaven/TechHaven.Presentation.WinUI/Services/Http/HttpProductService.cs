using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to delete product");
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

                var response = await _httpClient.PostAsync("api/Image/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    string imageUrl = await response.Content.ReadAsStringAsync();

                    // SỬA Ở ĐÂY: Khởi tạo thủ công
                    return new ResponseWrapper<string>
                    {
                        Success = true,
                        Data = imageUrl,
                        Message = "Upload thành công"
                    };
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

    }
}
