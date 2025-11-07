using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Presentation.WinUI.Services.Interfaces;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockProductService : IProductService
    {
        private readonly List<ProductDto> _mockProducts;
        public MockProductService()
        {
            _mockProducts = new List<ProductDto>
            {
                new ProductDto
                {
                    ProductId = 1,
                    ProductName = "iPhone 15 Pro",
                    CategoryId = 1,
                    CategoryName = "Điện thoại",
                    BrandName = "Apple",
                    Color = "Titan Gray",
                    StorageCapacity = 256,
                    Processor = "A17 Pro",
                    ScreenSize = 6.1m,
                    BatteryCapacity = 3274,
                    ImageUrl = "./Assets/phone.jpg",
                    ImageGalleryJson = "[\"iphone15pro1.jpg\",\"iphone15pro2.jpg\"]",
                    SellPrice = 28990000,
                    StockQuantity = 10,
                    Description = "Flagship Apple 2025, hiệu năng cực mạnh",
                    IsDraft = false
                },
                new ProductDto
                {
                    ProductId = 2,
                    ProductName = "Samsung Galaxy S24 Ultra",
                    CategoryId = 1,
                    CategoryName = "Điện thoại",
                    BrandName = "Samsung",
                    Color = "Titanium Black",
                    StorageCapacity = 512,
                    Processor = "Snapdragon 8 Gen 3",
                    ScreenSize = 6.8m,
                    BatteryCapacity = 5000,
                    ImageUrl = "./Assets/phone.jpg",
                    ImageGalleryJson = "[\"s24ultra1.jpg\",\"s24ultra2.jpg\"]",
                    SellPrice = 25990000,
                    StockQuantity = 15,
                    Description = "Camera 200MP, hỗ trợ bút S-Pen",
                    IsDraft = false
                },
                new ProductDto
                {
                    ProductId = 3,
                    ProductName = "Xiaomi 14T",
                    CategoryId = 1,
                    CategoryName = "Điện thoại",
                    BrandName = "Xiaomi",
                    Color = "Mint Green",
                    StorageCapacity = 256,
                    Processor = "Dimensity 8300-Ultra",
                    ScreenSize = 6.67m,
                    BatteryCapacity = 5000,
                    ImageUrl = "./Assets/phone.jpg",
                    ImageGalleryJson = "[\"xiaomi14t1.jpg\",\"xiaomi14t2.jpg\"]",
                    SellPrice = 14990000,
                    StockQuantity = 20,
                    Description = "Cấu hình mạnh, giá dễ tiếp cận",
                    IsDraft = false
                }
            };
        }
        // Lấy tất cả sản phẩm
        public Task<List<ProductDto>> GetAllProductsAsync()
        {
            return Task.FromResult(_mockProducts);
        }

        // Lấy theo ID
        public Task<ProductDto?> GetProductsByIdAsync(int id)
        {
            var product = _mockProducts.FirstOrDefault(p => p.ProductId == id);
            return Task.FromResult(product);
        }

        // Tạo mới
        public Task<ProductDto> CreateProductsAsync(ProductCreateUpdateDto dto)
        {
            var newProduct = new ProductDto
            {
                ProductId = _mockProducts.Max(p => p.ProductId) + 1,
                ProductName = dto.ProductName,
                CategoryId = dto.CategoryId,
                CategoryName = "Điện thoại",
                BrandName = dto.BrandName,
                Color = dto.Color,
                StorageCapacity = dto.StorageCapacity,
                Processor = dto.Processor,
                ScreenSize = dto.ScreenSize,
                BatteryCapacity = dto.BatteryCapacity,
                ImageUrl = dto.ImageUrl,
                ImageGalleryJson = dto.ImageGalleryJson,
                SellPrice = dto.SellPrice,
                StockQuantity = dto.StockQuantity,
                Description = dto.Description,
                IsDraft = dto.IsDraft
            };
            _mockProducts.Add(newProduct);
            return Task.FromResult(newProduct);
        }

        // Cập nhật
        public Task<ProductDto?> UpdateProductsAsync(int id, ProductCreateUpdateDto dto)
        {
            var existing = _mockProducts.FirstOrDefault(p => p.ProductId == id);
            if (existing == null)
                return Task.FromResult<ProductDto?>(null);

            existing.ProductName = dto.ProductName;
            existing.CategoryId = dto.CategoryId;
            existing.BrandName = dto.BrandName;
            existing.Color = dto.Color;
            existing.StorageCapacity = dto.StorageCapacity;
            existing.Processor = dto.Processor;
            existing.ScreenSize = dto.ScreenSize;
            existing.BatteryCapacity = dto.BatteryCapacity;
            existing.ImageUrl = dto.ImageUrl;
            existing.ImageGalleryJson = dto.ImageGalleryJson;
            existing.SellPrice = dto.SellPrice;
            existing.StockQuantity = dto.StockQuantity;
            existing.Description = dto.Description;
            existing.IsDraft = dto.IsDraft;

            return Task.FromResult<ProductDto?>(existing);
        }

        // Xoá sản phẩm
        public Task<bool> DeleteProductsAsync(int id)
        {
            var existing = _mockProducts.FirstOrDefault(p => p.ProductId == id);
            if (existing == null) return Task.FromResult(false);
            _mockProducts.Remove(existing);
            return Task.FromResult(true);
        }

        // Truy vấn theo ProductQueryDto (lọc + sắp xếp + phân trang)
        public Task<List<ProductDto>> QueryProductsAsync(ProductQueryDto query)
        {
            IEnumerable<ProductDto> result = _mockProducts;

            // Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                result = result.Where(p =>
                    p.ProductName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description?.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Lọc theo danh mục
            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                result = result.Where(p =>
                    p.CategoryName?.Equals(query.CategoryName, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            // Lọc theo trạng thái draft
            if (query.IsDraft.HasValue)
            {
                result = result.Where(p => p.IsDraft == query.IsDraft.Value);
            }

            // Sắp xếp
            if (query.Sorting != null)
            {
                if (query.Sorting.SortBy?.Equals("price", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = query.Sorting.Desc
                        ? result.OrderByDescending(p => p.SellPrice)
                        : result.OrderBy(p => p.SellPrice);
                }
                else if (query.Sorting.SortBy?.Equals("name", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = query.Sorting.Desc
                        ? result.OrderByDescending(p => p.ProductName)
                        : result.OrderBy(p => p.ProductName);
                }
            }

            // Phân trang
            if (query.PageNumber > 0 && query.PageSize > 0)
            {
                result = result
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return Task.FromResult(result.ToList());
        }
    }
}
