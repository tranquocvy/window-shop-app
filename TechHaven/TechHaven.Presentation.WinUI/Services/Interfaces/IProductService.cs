using Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IProductService
    {
        Task<ResponseWrapper<List<ProductDto>>> GetAllProductsAsync();
        Task<ResponseWrapper<ProductDto>> GetProductsByIdAsync(int id);
        Task<ResponseWrapper<ProductDto>> CreateProductsAsync(ProductCreateUpdateDto dto);
        Task<ResponseWrapper<ProductDto>> UpdateProductsAsync(int id, ProductCreateUpdateDto dto);
        Task<ResponseWrapper<bool>> DeleteProductsAsync(int id);
        Task<ResponseWrapper<List<ProductDto>>> QueryProductsAsync(ProductQueryDto query);
    }
}
