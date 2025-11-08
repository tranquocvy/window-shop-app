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
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductsByIdAsync(int id);
        Task<ProductDto> CreateProductsAsync(ProductCreateUpdateDto dto);
        Task<ProductDto?> UpdateProductsAsync(int id, ProductCreateUpdateDto dto);
        Task<bool> DeleteProductsAsync(int id);
        Task<List<ProductDto>> QueryProductsAsync(ProductQueryDto query);

    }
}
