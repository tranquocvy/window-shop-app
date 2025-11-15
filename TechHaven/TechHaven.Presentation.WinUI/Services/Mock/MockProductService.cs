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
                    // ===== APPLE =====
                    new ProductDto { ProductId = 1, ProductName = "iPhone 15 Pro", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Titan Gray", StorageCapacity = 256, Processor = "A17 Pro", ScreenSize = 6.1m, BatteryCapacity = 3274, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone15pro1.jpg\",\"iphone15pro2.jpg\"]", SellPrice = 28990000, StockQuantity = 10, Description = "Flagship Apple 2025, hiệu năng cực mạnh", IsDraft = false },
                    new ProductDto { ProductId = 2, ProductName = "iPhone 15 Pro Max", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Natural Titanium", StorageCapacity = 512, Processor = "A17 Pro", ScreenSize = 6.7m, BatteryCapacity = 4422, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone15promax1.jpg\"]", SellPrice = 33990000, StockQuantity = 8, Description = "Phiên bản cao cấp nhất, camera 5x zoom", IsDraft = false },
                    new ProductDto { ProductId = 3, ProductName = "iPhone 15", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Blue", StorageCapacity = 128, Processor = "A16 Bionic", ScreenSize = 6.1m, BatteryCapacity = 3349, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone15blue1.jpg\"]", SellPrice = 21990000, StockQuantity = 12, Description = "Thiết kế mới, Dynamic Island", IsDraft = false },
                    new ProductDto { ProductId = 4, ProductName = "iPhone 15 Plus", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Pink", StorageCapacity = 256, Processor = "A16 Bionic", ScreenSize = 6.7m, BatteryCapacity = 4383, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone15plus1.jpg\"]", SellPrice = 24990000, StockQuantity = 9, Description = "Màn hình lớn, pin bền", IsDraft = false },
                    new ProductDto { ProductId = 5, ProductName = "iPhone 14 Pro", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Deep Purple", StorageCapacity = 256, Processor = "A16 Bionic", ScreenSize = 6.1m, BatteryCapacity = 3200, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone14pro1.jpg\"]", SellPrice = 26990000, StockQuantity = 6, Description = "Dynamic Island thế hệ đầu", IsDraft = false },
                    new ProductDto { ProductId = 6, ProductName = "iPhone 13", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Midnight", StorageCapacity = 128, Processor = "A15 Bionic", ScreenSize = 6.1m, BatteryCapacity = 3227, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone13black1.jpg\"]", SellPrice = 17990000, StockQuantity = 20, Description = "Hiệu năng vẫn mạnh mẽ", IsDraft = false },
                    new ProductDto { ProductId = 7, ProductName = "iPhone SE (2022)", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Red", StorageCapacity = 128, Processor = "A15 Bionic", ScreenSize = 4.7m, BatteryCapacity = 2018, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphonese2022.jpg\"]", SellPrice = 10990000, StockQuantity = 15, Description = "Giá mềm, chip mạnh", IsDraft = false },
                    new ProductDto { ProductId = 8, ProductName = "iPhone 14", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Starlight", StorageCapacity = 128, Processor = "A15 Bionic", ScreenSize = 6.1m, BatteryCapacity = 3279, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone14starlight.jpg\"]", SellPrice = 19990000, StockQuantity = 13, Description = "Thiết kế trẻ trung", IsDraft = false },
                    new ProductDto { ProductId = 9, ProductName = "iPhone 13 Mini", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Green", StorageCapacity = 128, Processor = "A15 Bionic", ScreenSize = 5.4m, BatteryCapacity = 2406, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone13mini.jpg\"]", SellPrice = 14990000, StockQuantity = 14, Description = "Nhỏ gọn, hiệu năng tốt", IsDraft = false },
                    new ProductDto { ProductId = 10, ProductName = "iPhone 14 Plus", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Apple", Color = "Yellow", StorageCapacity = 256, Processor = "A15 Bionic", ScreenSize = 6.7m, BatteryCapacity = 4323, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"iphone14plus.jpg\"]", SellPrice = 22990000, StockQuantity = 11, Description = "Pin tốt, màn hình lớn", IsDraft = false },

                    // ===== SAMSUNG =====
                    new ProductDto { ProductId = 11, ProductName = "Samsung Galaxy S24 Ultra", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Titanium Black", StorageCapacity = 512, Processor = "Snapdragon 8 Gen 3", ScreenSize = 6.8m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"s24ultra1.jpg\"]", SellPrice = 25990000, StockQuantity = 15, Description = "Camera 200MP, hỗ trợ bút S-Pen", IsDraft = false },
                    new ProductDto { ProductId = 12, ProductName = "Samsung Galaxy S24+", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Cobalt Violet", StorageCapacity = 256, Processor = "Snapdragon 8 Gen 3", ScreenSize = 6.7m, BatteryCapacity = 4700, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"s24plus1.jpg\"]", SellPrice = 23990000, StockQuantity = 12, Description = "Màn lớn, hiệu năng mạnh", IsDraft = false },
                    new ProductDto { ProductId = 13, ProductName = "Samsung Galaxy S24", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Marble Gray", StorageCapacity = 256, Processor = "Snapdragon 8 Gen 3", ScreenSize = 6.2m, BatteryCapacity = 4000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"s24gray1.jpg\"]", SellPrice = 19990000, StockQuantity = 20, Description = "Nhỏ gọn, cao cấp", IsDraft = false },
                    new ProductDto { ProductId = 14, ProductName = "Samsung Galaxy Z Fold5", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Icy Blue", StorageCapacity = 512, Processor = "Snapdragon 8 Gen 2", ScreenSize = 7.6m, BatteryCapacity = 4400, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"zfold5.jpg\"]", SellPrice = 40990000, StockQuantity = 5, Description = "Gập ngang, hiệu năng mạnh", IsDraft = false },
                    new ProductDto { ProductId = 15, ProductName = "Samsung Galaxy Z Flip5", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Mint", StorageCapacity = 256, Processor = "Snapdragon 8 Gen 2", ScreenSize = 6.7m, BatteryCapacity = 3700, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"zflip5.jpg\"]", SellPrice = 25990000, StockQuantity = 10, Description = "Gập dọc thời trang", IsDraft = false },
                    new ProductDto { ProductId = 16, ProductName = "Samsung Galaxy A55", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Awesome Navy", StorageCapacity = 256, Processor = "Exynos 1480", ScreenSize = 6.6m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"a55.jpg\"]", SellPrice = 9990000, StockQuantity = 25, Description = "Tầm trung mới 2025", IsDraft = false },
                    new ProductDto { ProductId = 17, ProductName = "Samsung Galaxy A35", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Ice Blue", StorageCapacity = 128, Processor = "Exynos 1380", ScreenSize = 6.6m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"a35.jpg\"]", SellPrice = 7990000, StockQuantity = 30, Description = "Phân khúc phổ thông", IsDraft = false },
                    new ProductDto { ProductId = 18, ProductName = "Samsung Galaxy S23 Ultra", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Cream", StorageCapacity = 512, Processor = "Snapdragon 8 Gen 2", ScreenSize = 6.8m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"s23ultra.jpg\"]", SellPrice = 22990000, StockQuantity = 10, Description = "Vẫn mạnh, giá tốt", IsDraft = false },
                    new ProductDto { ProductId = 19, ProductName = "Samsung Galaxy A15", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Light Blue", StorageCapacity = 128, Processor = "Helio G99", ScreenSize = 6.5m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"a15.jpg\"]", SellPrice = 4990000, StockQuantity = 35, Description = "Giá rẻ, pin trâu", IsDraft = false },
                    new ProductDto { ProductId = 20, ProductName = "Samsung Galaxy M14", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Samsung", Color = "Dark Blue", StorageCapacity = 128, Processor = "Exynos 1330", ScreenSize = 6.6m, BatteryCapacity = 6000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"m14.jpg\"]", SellPrice = 3990000, StockQuantity = 40, Description = "Dòng pin khủng, giá thấp", IsDraft = false },

                    // ===== XIAOMI =====
                    new ProductDto { ProductId = 21, ProductName = "Xiaomi 14T", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Mint Green", StorageCapacity = 256, Processor = "Dimensity 8300-Ultra", ScreenSize = 6.67m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"xiaomi14t1.jpg\"]", SellPrice = 14990000, StockQuantity = 20, Description = "Cấu hình mạnh, giá dễ tiếp cận", IsDraft = false },
                    new ProductDto { ProductId = 22, ProductName = "Xiaomi 14 Ultra", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "White", StorageCapacity = 512, Processor = "Snapdragon 8 Gen 3", ScreenSize = 6.73m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"xiaomi14ultra.jpg\"]", SellPrice = 24990000, StockQuantity = 10, Description = "Camera Leica, flagship", IsDraft = false },
                    new ProductDto { ProductId = 23, ProductName = "Xiaomi 13T Pro", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Alpine Blue", StorageCapacity = 512, Processor = "Dimensity 9200+", ScreenSize = 6.67m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"13tpro.jpg\"]", SellPrice = 16990000, StockQuantity = 18, Description = "Camera Leica, giá tốt", IsDraft = false },
                    new ProductDto { ProductId = 24, ProductName = "Redmi Note 13 Pro+", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Aurora Purple", StorageCapacity = 256, Processor = "Dimensity 7200-Ultra", ScreenSize = 6.67m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"note13proplus.jpg\"]", SellPrice = 9990000, StockQuantity = 25, Description = "Tầm trung hiệu năng tốt", IsDraft = false },
                    new ProductDto { ProductId = 25, ProductName = "Redmi Note 13", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Ice Blue", StorageCapacity = 128, Processor = "Snapdragon 685", ScreenSize = 6.6m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"note13.jpg\"]", SellPrice = 5990000, StockQuantity = 40, Description = "Phù hợp học sinh sinh viên", IsDraft = false },
                    new ProductDto { ProductId = 26, ProductName = "Redmi Note 12", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Graphite Gray", StorageCapacity = 128, Processor = "Snapdragon 4 Gen 1", ScreenSize = 6.6m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"note12.jpg\"]", SellPrice = 4990000, StockQuantity = 45, Description = "Giá rẻ, ổn định", IsDraft = false },
                    new ProductDto { ProductId = 27, ProductName = "POCO F6 Pro", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Black", StorageCapacity = 512, Processor = "Snapdragon 8 Gen 2", ScreenSize = 6.67m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"pocof6pro.jpg\"]", SellPrice = 15990000, StockQuantity = 22, Description = "Hiệu năng cao, giá tốt", IsDraft = false },
                    new ProductDto { ProductId = 28, ProductName = "POCO X6 Pro", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Yellow", StorageCapacity = 256, Processor = "Dimensity 8300 Ultra", ScreenSize = 6.67m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"pocox6pro.jpg\"]", SellPrice = 10990000, StockQuantity = 28, Description = "Máy gaming tầm trung", IsDraft = false },
                    new ProductDto { ProductId = 29, ProductName = "Redmi 13C", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Ocean Blue", StorageCapacity = 128, Processor = "Helio G85", ScreenSize = 6.74m, BatteryCapacity = 5000, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"redmi13c.jpg\"]", SellPrice = 3490000, StockQuantity = 50, Description = "Giá rẻ, phù hợp nhu cầu cơ bản", IsDraft = false },
                    new ProductDto { ProductId = 30, ProductName = "Xiaomi Civi 4 Pro", CategoryId = 1, CategoryName = "Điện thoại", BrandName = "Xiaomi", Color = "Pink", StorageCapacity = 256, Processor = "Snapdragon 8s Gen 3", ScreenSize = 6.55m, BatteryCapacity = 4700, ImageUrl = "./Assets/phone.jpg", ImageGalleryJson = "[\"civi4pro.jpg\"]", SellPrice = 13990000, StockQuantity = 16, Description = "Mỏng nhẹ, thời trang", IsDraft = false }
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
