using Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/products";

        public HttpProductService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ResponseWrapper<List<ProductDto>>> GetAllProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<List<ProductDto>>>(BaseUrl);
                return response ?? new ResponseWrapper<List<ProductDto>> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<List<ProductDto>> { Success = false, Message = "Failed to retrieve products", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<ProductDto>> GetProductsByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<ProductDto>>($"{BaseUrl}/{id}");
                return response ?? new ResponseWrapper<ProductDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<ProductDto> { Success = false, Message = "Failed to retrieve product", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<ProductDto>> CreateProductsAsync(ProductCreateUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<ProductDto> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<ProductDto>>();
                return wrapper ?? new ResponseWrapper<ProductDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<ProductDto> { Success = false, Message = "Failed to create product", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<ProductDto>> UpdateProductsAsync(int id, ProductCreateUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<ProductDto> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<ProductDto>>();
                return wrapper ?? new ResponseWrapper<ProductDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<ProductDto> { Success = false, Message = "Failed to update product", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<bool>> DeleteProductsAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<bool> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<bool>>();
                return wrapper ?? new ResponseWrapper<bool> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<bool> { Success = false, Message = "Failed to delete product", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<List<ProductDto>>> QueryProductsAsync(ProductQueryDto query)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/query", query);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<List<ProductDto>> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<List<ProductDto>>>();
                return wrapper ?? new ResponseWrapper<List<ProductDto>> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<List<ProductDto>> { Success = false, Message = "Failed to query products", Errors = new List<string> { ex.Message } };
            }
        }
    }
}
