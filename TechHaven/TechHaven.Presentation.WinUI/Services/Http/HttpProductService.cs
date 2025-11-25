using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
        private const string BaseUrl = "api/products";

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
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            return await response.EnsureSuccessAndReadWrapperAsync<ProductDto>("Failed to create product");
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
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/query", query);

            return await response.EnsureSuccessAndReadWrapperAsync<PagingResponse<ProductDto>>(
                "Failed to query products"
            );
        }

    }
}
