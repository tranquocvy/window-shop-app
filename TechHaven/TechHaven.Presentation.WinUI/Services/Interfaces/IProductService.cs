using System.Collections.Generic;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;
using System.IO;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IProductService
    {
        Task<ResponseWrapper<ProductDto>> GetProductsByIdAsync(int id);
        Task<ResponseWrapper<ProductDto>> CreateProductsAsync(ProductUpsertRequest dto);
        Task<ResponseWrapper<ProductDto>> UpdateProductsAsync(int id, ProductUpsertRequest dto);
        Task<ResponseWrapper<bool>> DeleteProductsAsync(int id);
        public Task<ResponseWrapper<PagingResponse<ProductDto>>> QueryProductsAsync(ProductListQueryDto query);

        Task<ResponseWrapper<string>> UploadImageAsync(Stream stream, string fileName, string contentType);
        Task<ResponseWrapper<bool>> DeleteImageAsync(string imageUrl);

    }
}
