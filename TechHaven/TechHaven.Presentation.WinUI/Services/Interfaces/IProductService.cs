using System.Collections.Generic;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IProductService
    {
        Task<ResponseWrapper<ProductDto>> GetProductsByIdAsync(int id);
        Task<ResponseWrapper<ProductDto>> CreateProductsAsync(ProductCreateUpdateDto dto);
        Task<ResponseWrapper<ProductDto>> UpdateProductsAsync(int id, ProductCreateUpdateDto dto);
        Task<ResponseWrapper<bool>> DeleteProductsAsync(int id);
        public Task<ResponseWrapper<PagingResponse<ProductDto>>> QueryProductsAsync(ProductQueryDto query);

    }
}
