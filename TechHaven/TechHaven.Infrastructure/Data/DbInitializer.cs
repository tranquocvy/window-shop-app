using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Infrastructure.Persistence;

namespace TechHaven.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied.");

        // 1. Tạo Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { RoleId = 1, RoleName = "Admin", Description = "System Administrator" },
                new Role { RoleId = 2, RoleName = "Seller", Description = "Sales Staff" }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default roles.");
        }

        // 2. Tạo Users (password: "123456")
        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new User
                {
                    UserId = 1,
                    UserFullName = "Nguyễn Phúc Hoàng",
                    Email = "nphuchoang.itus@gmail.com",
                    UserName = "nphoang",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 1,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 2,
                    UserFullName = "Nguyễn Văn Bình Dương",
                    Email = "dn016777@gmail.com",
                    UserName = "nvbduong",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 1,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 3,
                    UserFullName = "Nguyễn Phúc Hậu",
                    Email = "phuchau.2005.vlg@gmail.com",
                    UserName = "nphau",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 1,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 4,
                    UserFullName = "Nguyễn Phúc Hậu",
                    Email = "phuchau.2005.vlg@gmail.com",
                    UserName = "nphau_seller",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 2,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 5,
                    UserFullName = "Nguyễn Khắc Vượng",
                    Email = "khacvuong2707@gmail.com",
                    UserName = "nkvuong",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 1,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 6,
                    UserFullName = "Nguyễn Khắc Vượng",
                    Email = "khacvuong2707@gmail.com",
                    UserName = "nkvuong_seller",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 2,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 7,
                    UserFullName = "Trần Quốc Vỹ",
                    Email = "quocvy23072005@gmail.com",
                    UserName = "tqvy",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 1,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 8,
                    UserFullName = "Trần Quốc Vỹ",
                    Email = "quocvy23072005@gmail.com",
                    UserName = "tqvy_seller",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 2,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default users.");
        }

        // 3. Seed Products
        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "iPhone 15",
                    BrandName = "Apple",
                    Color = "Blue",
                    StorageCapacity = 128,
                    Processor = "A16 Bionic",
                    ScreenSize = 6.1m,
                    BatteryCapacity = 3349,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/303430/iphone-15-blue-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/303430/iphone-15-blue-2-600x600.jpg\"]",
                    CostPrice = 19000000,
                    SellPrice = 22990000,
                    StockQuantity = 20,
                    Description = "iPhone 15 với Dynamic Island, camera 48MP.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "iPhone 15 Plus",
                    BrandName = "Apple",
                    Color = "Pink",
                    StorageCapacity = 256,
                    Processor = "A16 Bionic",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 4383,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/303429/iphone-15-plus-pink-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/303429/iphone-15-plus-pink-2-600x600.jpg\"]",
                    CostPrice = 21000000,
                    SellPrice = 25990000,
                    StockQuantity = 14,
                    Description = "Phiên bản màn lớn của iPhone 15.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "iPhone 15 Pro Max",
                    BrandName = "Apple",
                    Color = "Natural Titanium",
                    StorageCapacity = 256,
                    Processor = "A17 Pro",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 4422,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/303421/iphone-15-pro-max-natural-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/303421/iphone-15-pro-max-2-600x600.jpg\"]",
                    CostPrice = 31000000,
                    SellPrice = 34990000,
                    StockQuantity = 8,
                    Description = "Flagship cao cấp nhất dòng iPhone 15.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "iPhone 14",
                    BrandName = "Apple",
                    Color = "Purple",
                    StorageCapacity = 128,
                    Processor = "A15 Bionic",
                    ScreenSize = 6.1m,
                    BatteryCapacity = 3279,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/271701/iphone-14-purple-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/271701/iphone-14-purple-2-600x600.jpg\"]",
                    CostPrice = 15000000,
                    SellPrice = 17990000,
                    StockQuantity = 35,
                    Description = "iPhone 14 phiên bản phổ thông, camera tốt.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "iPhone 13",
                    BrandName = "Apple",
                    Color = "Green",
                    StorageCapacity = 128,
                    Processor = "A15 Bionic",
                    ScreenSize = 6.1m,
                    BatteryCapacity = 3240,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/303919/iphone-13-green-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/303919/iphone-13-green-2-600x600.jpg\"]",
                    CostPrice = 12500000,
                    SellPrice = 14990000,
                    StockQuantity = 40,
                    Description = "iPhone 13 – hiệu năng vẫn mạnh, giá mềm.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // ----------- SAMSUNG ------------

                new Product
                {
                    ProductName = "Samsung Galaxy A55",
                    BrandName = "Samsung",
                    Color = "Awesome Iceblue",
                    StorageCapacity = 128,
                    Processor = "Exynos 1480",
                    ScreenSize = 6.5m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/302024/samsung-galaxy-a55-iceblue-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/302024/samsung-galaxy-a55-2-600x600.jpg\"]",
                    CostPrice = 8500000,
                    SellPrice = 9990000,
                    StockQuantity = 45,
                    Description = "Galaxy A55 – màn hình đẹp, hiệu năng tốt tầm trung.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Samsung Galaxy A35",
                    BrandName = "Samsung",
                    Color = "Awesome Navy",
                    StorageCapacity = 128,
                    Processor = "Exynos 1380",
                    ScreenSize = 6.6m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/301794/samsung-galaxy-a35-navy-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/301794/samsung-galaxy-a35-2-600x600.jpg\"]",
                    CostPrice = 6500000,
                    SellPrice = 7990000,
                    StockQuantity = 60,
                    Description = "Galaxy A35 – lựa chọn tầm trung có màn đẹp và pin trâu.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Samsung Galaxy Z Flip 5",
                    BrandName = "Samsung",
                    Color = "Mint",
                    StorageCapacity = 256,
                    Processor = "Snapdragon 8 Gen 2",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 3700,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/289701/samsung-galaxy-z-flip5-mint-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/289701/samsung-galaxy-z-flip5-2-600x600.jpg\"]",
                    CostPrice = 21000000,
                    SellPrice = 24990000,
                    StockQuantity = 12,
                    Description = "Galaxy Z Flip 5 – điện thoại gập nhỏ gọn, thiết kế đẹp.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Samsung Galaxy Z Fold 5",
                    BrandName = "Samsung",
                    Color = "Icy Blue",
                    StorageCapacity = 512,
                    Processor = "Snapdragon 8 Gen 2",
                    ScreenSize = 7.6m,
                    BatteryCapacity = 4400,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/289700/samsung-galaxy-z-fold5-blue-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/289700/samsung-galaxy-z-fold5-2-600x600.jpg\"]",
                    CostPrice = 35000000,
                    SellPrice = 39990000,
                    StockQuantity = 10,
                    Description = "Galaxy Z Fold 5 – flagship gập với màn lớn.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // ---------- XIAOMI ----------

                new Product
                {
                    ProductName = "Xiaomi 14 Ultra",
                    BrandName = "Xiaomi",
                    Color = "White",
                    StorageCapacity = 512,
                    Processor = "Snapdragon 8 Gen 3",
                    ScreenSize = 6.73m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/305887/xiaomi-14-ultra-white-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/305887/xiaomi-14-ultra-2-600x600.jpg\"]",
                    CostPrice = 26000000,
                    SellPrice = 29990000,
                    StockQuantity = 20,
                    Description = "Xiaomi 14 Ultra – camera Leica, cấu hình cực mạnh.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Xiaomi 13 Pro",
                    BrandName = "Xiaomi",
                    Color = "Ceramic White",
                    StorageCapacity = 256,
                    Processor = "Snapdragon 8 Gen 2",
                    ScreenSize = 6.73m,
                    BatteryCapacity = 4820,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/280742/xiaomi-13-pro-white-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/280742/xiaomi-13-pro-white-2-600x600.jpg\"]",
                    CostPrice = 20000000,
                    SellPrice = 23990000,
                    StockQuantity = 18,
                    Description = "Máy flagship Xiaomi 13 Pro, camera Leica.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Xiaomi Redmi Note 12",
                    BrandName = "Xiaomi",
                    Color = "Onyx Gray",
                    StorageCapacity = 128,
                    Processor = "Snapdragon 685",
                    ScreenSize = 6.67m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/301087/xiaomi-redmi-note-12-gray-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/301087/xiaomi-redmi-note-12-gray-2-600x600.jpg\"]",
                    CostPrice = 3500000,
                    SellPrice = 4490000,
                    StockQuantity = 70,
                    Description = "Redmi Note 12 – giá rẻ, pin trâu.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- ONEPLUS ---------

                new Product
                {
                    ProductName = "OnePlus 11",
                    BrandName = "OnePlus",
                    Color = "Titan Black",
                    StorageCapacity = 256,
                    Processor = "Snapdragon 8 Gen 2",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/280778/oneplus-11-black-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/280778/oneplus-11-black-2-600x600.jpg\"]",
                    CostPrice = 17000000,
                    SellPrice = 19990000,
                    StockQuantity = 15,
                    Description = "OnePlus 11 – flagship ổn định, OxygenOS mượt.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "OnePlus Nord CE 4",
                    BrandName = "OnePlus",
                    Color = "Dark Chrome",
                    StorageCapacity = 128,
                    Processor = "Snapdragon 782G",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 5500,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/307001/oneplus-nord-ce4-black-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/307001/oneplus-nord-ce4-2-600x600.jpg\"]",
                    CostPrice = 6000000,
                    SellPrice = 7490000,
                    StockQuantity = 36,
                    Description = "Nord CE4 – hiệu năng tốt trong tầm giá.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- GOOGLE ---------

                new Product
                {
                    ProductName = "Google Pixel 8 Pro",
                    BrandName = "Google",
                    Color = "Porcelain",
                    StorageCapacity = 256,
                    Processor = "Google Tensor G3",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 5050,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/308647/google-pixel-8-pro-porcelain-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/308647/google-pixel-8-pro-2-600x600.jpg\"]",
                    CostPrice = 23000000,
                    SellPrice = 26990000,
                    StockQuantity = 12,
                    Description = "Pixel 8 Pro – ảnh đẹp nhất Android, AI mạnh.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Google Pixel 7",
                    BrandName = "Google",
                    Color = "Lemongrass",
                    StorageCapacity = 128,
                    Processor = "Google Tensor G2",
                    ScreenSize = 6.3m,
                    BatteryCapacity = 4355,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/308645/google-pixel-7-yellow-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/308645/google-pixel-7-2-600x600.jpg\"]",
                    CostPrice = 9500000,
                    SellPrice = 11990000,
                    StockQuantity = 28,
                    Description = "Pixel 7 – camera xuất sắc, Android thuần.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- OPPO ----------

                new Product
                {
                    ProductName = "Oppo Reno11",
                    BrandName = "Oppo",
                    Color = "Green",
                    StorageCapacity = 256,
                    Processor = "Dimensity 7050",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/306830/oppo-reno11-green-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/306830/oppo-reno11-green-2-600x600.jpg\"]",
                    CostPrice = 8500000,
                    SellPrice = 10990000,
                    StockQuantity = 30,
                    Description = "Reno11 – chuyên chụp chân dung, thiết kế đẹp.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Oppo A78",
                    BrandName = "Oppo",
                    Color = "Aqua Green",
                    StorageCapacity = 128,
                    Processor = "Snapdragon 680",
                    ScreenSize = 6.43m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/303864/oppo-a78-green-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/303864/oppo-a78-green-2-600x600.jpg\"]",
                    CostPrice = 4500000,
                    SellPrice = 5990000,
                    StockQuantity = 50,
                    Description = "A78 – pin trâu, sạc nhanh 67W.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- REALME ---------

                new Product
                {
                    ProductName = "Realme 11 Pro+",
                    BrandName = "Realme",
                    Color = "Sunrise Beige",
                    StorageCapacity = 256,
                    Processor = "Dimensity 7050",
                    ScreenSize = 6.7m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/302528/realme-11-pro-plus-beige-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/302528/realme-11-pro-plus-2-600x600.jpg\"]",
                    CostPrice = 9000000,
                    SellPrice = 11990000,
                    StockQuantity = 32,
                    Description = "Realme 11 Pro+ – camera 200MP.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Realme C55",
                    BrandName = "Realme",
                    Color = "Rainy Night",
                    StorageCapacity = 128,
                    Processor = "Helio G88",
                    ScreenSize = 6.72m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/301638/realme-c55-black-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/301638/realme-c55-black-2-600x600.jpg\"]",
                    CostPrice = 3000000,
                    SellPrice = 4490000,
                    StockQuantity = 70,
                    Description = "Realme C55 – giá rẻ, hiệu năng ổn cho học sinh.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- VIVO --------

                new Product
                {
                    ProductName = "Vivo V29",
                    BrandName = "Vivo",
                    Color = "Blue",
                    StorageCapacity = 256,
                    Processor = "Snapdragon 778G",
                    ScreenSize = 6.78m,
                    BatteryCapacity = 4600,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/305602/vivo-v29-blue-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/305602/vivo-v29-blue-2-600x600.jpg\"]",
                    CostPrice = 9000000,
                    SellPrice = 11990000,
                    StockQuantity = 20,
                    Description = "Vivo V29 – chuyên chụp selfie và chân dung.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Vivo Y36",
                    BrandName = "Vivo",
                    Color = "Gold",
                    StorageCapacity = 128,
                    Processor = "Snapdragon 680",
                    ScreenSize = 6.64m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/306103/vivo-y36-gold-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/306103/vivo-y36-gold-2-600x600.jpg\"]",
                    CostPrice = 4500000,
                    SellPrice = 5690000,
                    StockQuantity = 48,
                    Description = "Vivo Y36 – điện thoại tầm trung giá tốt.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- MOTOROLA --------

                new Product
                {
                    ProductName = "Motorola Edge 40",
                    BrandName = "Motorola",
                    Color = "Nebula Green",
                    StorageCapacity = 256,
                    Processor = "Dimensity 8020",
                    ScreenSize = 6.55m,
                    BatteryCapacity = 4400,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/306765/motorola-edge40-green-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/306765/motorola-edge40-green-2-600x600.jpg\"]",
                    CostPrice = 8000000,
                    SellPrice = 9990000,
                    StockQuantity = 22,
                    Description = "Motorola Edge 40 – thiết kế đẹp, màn 144Hz.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- NOKIA ---------

                new Product
                {
                    ProductName = "Nokia G22",
                    BrandName = "Nokia",
                    Color = "Blue",
                    StorageCapacity = 128,
                    Processor = "Unisoc T606",
                    ScreenSize = 6.5m,
                    BatteryCapacity = 5050,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/304118/nokia-g22-blue-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/304118/nokia-g22-blue-2-600x600.jpg\"]",
                    CostPrice = 2500000,
                    SellPrice = 3490000,
                    StockQuantity = 60,
                    Description = "Nokia G22 – bền, pin trâu, dễ sửa chữa.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- SONY ---------

                new Product
                {
                    ProductName = "Sony Xperia 5 V",
                    BrandName = "Sony",
                    Color = "Black",
                    StorageCapacity = 256,
                    Processor = "Snapdragon 8 Gen 2",
                    ScreenSize = 6.1m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/304998/sony-xperia-5v-black-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/304998/sony-xperia-5v-black-2-600x600.jpg\"]",
                    CostPrice = 21000000,
                    SellPrice = 24990000,
                    StockQuantity = 8,
                    Description = "Sony Xperia 5 V – nhỏ gọn, camera Pro.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- HONOR ---------

                new Product
                {
                    ProductName = "Honor X9b",
                    BrandName = "Honor",
                    Color = "Sunrise Orange",
                    StorageCapacity = 256,
                    Processor = "Snapdragon 6 Gen 1",
                    ScreenSize = 6.78m,
                    BatteryCapacity = 5800,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/307132/honor-x9b-orange-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/307132/honor-x9b-orange-2-600x600.jpg\"]",
                    CostPrice = 6500000,
                    SellPrice = 7990000,
                    StockQuantity = 42,
                    Description = "Honor X9b – siêu bền, pin cực lớn.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },

                // -------- INFINIX ---------

                new Product
                {
                    ProductName = "Infinix Hot 30i",
                    BrandName = "Infinix",
                    Color = "Diamond White",
                    StorageCapacity = 128,
                    Processor = "Unisoc T606",
                    ScreenSize = 6.56m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/300793/infinix-hot30i-white-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/300793/infinix-hot30i-white-2-600x600.jpg\"]",
                    CostPrice = 2000000,
                    SellPrice = 2990000,
                    StockQuantity = 80,
                    Description = "Infinix Hot 30i – giá rẻ, phù hợp sinh viên.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                },
                new Product
                {
                    ProductName = "Infinix Note 30",
                    BrandName = "Infinix",
                    Color = "Black",
                    StorageCapacity = 256,
                    Processor = "Helio G99",
                    ScreenSize = 6.78m,
                    BatteryCapacity = 5000,
                    ImageUrl = "https://cdn.tgdd.vn/Products/Images/42/300794/infinix-note-30-black-600x600.jpg",
                    ImageGalleryJson = "[\"https://cdn.tgdd.vn/Products/Images/42/300794/infinix-note-30-black-2-600x600.jpg\"]",
                    CostPrice = 3500000,
                    SellPrice = 4990000,
                    StockQuantity = 50,
                    Description = "Infinix Note 30 – màn lớn, loa kép.",
                    IsDraft = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                }

            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded sample products.");
        }

        // 4. Seed Customers
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new Customer { CustomerName = "Nguyễn Văn An", PhoneNumber = "0901234567", Email = "a.nguyen@example.com", Address = "Hà Nội", Type = CustomerType.Regular, TotalPurchased = 12000000, Note = "Khách hàng thường xuyên", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Trần Thị Bưởi", PhoneNumber = "0912345678", Email = "b.tran@example.com", Address = "TP.HCM", Type = CustomerType.Student, TotalPurchased = 3500000, Note = "Sinh viên, được giảm 10%", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Phạm Minh Chính", PhoneNumber = "0988888888", Email = "c.pham@example.com", Address = "Đà Nẵng", Type = CustomerType.VIP, TotalPurchased = 75000000, Note = "Khách VIP, ưu tiên hỗ trợ", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Lê Thị Dung", PhoneNumber = "0901112233", Email = "d.le@example.com", Address = "Hải Phòng", Type = CustomerType.Regular, TotalPurchased = 8200000, Note = "Khách mới", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Vũ Văn Em", PhoneNumber = "0911223344", Email = "e.vu@example.com", Address = "Cần Thơ", Type = CustomerType.Student, TotalPurchased = 1500000, Note = "Sinh viên, hay mua online", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Ngô Thị Fúc", PhoneNumber = "0922334455", Email = "f.ngo@example.com", Address = "Hà Nội", Type = CustomerType.VIP, TotalPurchased = 60000000, Note = "Khách VIP", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Đặng Văn Giang", PhoneNumber = "0933445566", Email = "g.dang@example.com", Address = "TP.HCM", Type = CustomerType.Regular, TotalPurchased = 15000000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Phan Thị Hoàng", PhoneNumber = "0944556677", Email = "h.phan@example.com", Address = "Đà Nẵng", Type = CustomerType.Student, TotalPurchased = 2800000, Note = "Mua theo nhóm", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Trương Minh Im", PhoneNumber = "0955667788", Email = "i.truong@example.com", Address = "Hải Phòng", Type = CustomerType.VIP, TotalPurchased = 90000000, Note = "Khách VIP", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Bùi Thị Jin", PhoneNumber = "0966778899", Email = "j.bui@example.com", Address = "Cần Thơ", Type = CustomerType.Regular, TotalPurchased = 5000000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Nguyễn Văn Kiệt", PhoneNumber = "0977889900", Email = "k.nguyen@example.com", Address = "Hà Nội", Type = CustomerType.Student, TotalPurchased = 4500000, Note = "Đang theo học lớp học online", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Trần Thị Long", PhoneNumber = "0988990011", Email = "l.tran@example.com", Address = "TP.HCM", Type = CustomerType.Regular, TotalPurchased = 7300000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Phạm Minh Minh", PhoneNumber = "0999001122", Email = "m.pham@example.com", Address = "Đà Nẵng", Type = CustomerType.VIP, TotalPurchased = 120000000, Note = "Khách VIP thân thiết", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Lê Thị Ninh", PhoneNumber = "0901122334", Email = "n.le@example.com", Address = "Hải Phòng", Type = CustomerType.Regular, TotalPurchased = 6500000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Vũ Văn Ong", PhoneNumber = "0912233445", Email = "o.vu@example.com", Address = "Cần Thơ", Type = CustomerType.Student, TotalPurchased = 3200000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Ngô Thị Phúc", PhoneNumber = "0923344556", Email = "p.ngo@example.com", Address = "Hà Nội", Type = CustomerType.VIP, TotalPurchased = 80000000, Note = "Khách VIP ưu tiên", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Đặng Văn Quân", PhoneNumber = "0934455667", Email = "q.dang@example.com", Address = "TP.HCM", Type = CustomerType.Regular, TotalPurchased = 9800000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Phan Thị Răn", PhoneNumber = "0945566778", Email = "r.phan@example.com", Address = "Đà Nẵng", Type = CustomerType.Student, TotalPurchased = 2500000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Trương Minh Sắn", PhoneNumber = "0956677889", Email = "s.truong@example.com", Address = "Hải Phòng", Type = CustomerType.VIP, TotalPurchased = 100000000, Note = "Khách VIP thân thiết", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Bùi Thị Tinh", PhoneNumber = "0967788990", Email = "t.bui@example.com", Address = "Cần Thơ", Type = CustomerType.Regular, TotalPurchased = 5500000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Nguyễn Văn Ung", PhoneNumber = "0978899001", Email = "u.nguyen@example.com", Address = "Hà Nội", Type = CustomerType.Student, TotalPurchased = 4700000, Note = "", CreatedAt = DateTime.UtcNow },
                new Customer { CustomerName = "Trần Thị Vũ", PhoneNumber = "0989900112", Email = "v.tran@example.com", Address = "TP.HCM", Type = CustomerType.VIP, TotalPurchased = 110000000, Note = "Khách VIP lâu năm", CreatedAt = DateTime.UtcNow }
            };
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded sample customers.");
        }
    }
}