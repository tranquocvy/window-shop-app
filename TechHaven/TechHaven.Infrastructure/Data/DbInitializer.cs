using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Infrastructure.Persistence;

namespace TechHaven.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Apply migrations
        await context.Database.MigrateAsync();

        // Kiểm tra đã có data chưa
        if (await context.Users.AnyAsync())
        {
            return; // Database đã có data
        }

        // 1. Tạo Roles
        var roles = new List<Role>
        {
            new Role
            {
                RoleId = 1,
                RoleName = "Admin",
                Description = "System Administrator"
            },
            new Role
            {
                RoleId = 2,
                RoleName = "Seller",
                Description = "Sales Staff"
            }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // 2. Tạo Users (password: "123456")
        // Password hash được tạo bằng BCrypt với workFactor = 12
        var users = new List<User>
        {
            new User
            {
                UserId = 1,
                UserFullName = "Admin User",
                Email = "admin@techhaven.com",
                UserName = "admin",
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
                UserFullName = "Seller User",
                Email = "seller@techhaven.com",
                UserName = "seller01",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                RoleId = 2,
                IsActive = true,
                HasSeenGuide = false,
                CreatedAt = DateTime.UtcNow,
                ActivatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 3,
                UserFullName = "Inactive User",
                Email = "inactive@techhaven.com",
                UserName = "inactive",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                RoleId = 2,
                IsActive = false,
                HasSeenGuide = false,
                CreatedAt = DateTime.UtcNow,
                ActivatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        Console.WriteLine("Seed data created successfully!");
        Console.WriteLine("\\nTest Accounts:");
        Console.WriteLine("1. Username: admin | Password: 123456 | Role: Admin");
        Console.WriteLine("2. Username: seller01 | Password: 123456 | Role: Seller");
        Console.WriteLine("3. Username: inactive | Password: 123456 | Role: Seller (Inactive)");

        // Kiểm tra nếu đã có dữ liệu
        if (await context.Products.AnyAsync())
        {
            return; // Database đã có dữ liệu
        }

        // Seed dữ liệu cho bảng Products
        var products = new List<Product>
        {
            new Product
            {
                ProductName = "iPhone 15 Pro",
                BrandName = "Apple",
                Color = "Titan Gray",
                StorageCapacity = 256,
                Processor = "A17 Pro",
                ScreenSize = 6.1m,
                BatteryCapacity = 3274,
                ImageUrl = "https://example.com/iphone15pro.jpg",
                ImageGalleryJson = "[\"iphone15pro1.jpg\",\"iphone15pro2.jpg\"]",
                CostPrice = 25000000,
                SellPrice = 28990000,
                StockQuantity = 10,
                Description = "Flagship Apple 2025, hiệu năng cực mạnh",
                IsDraft = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            },
            new Product
            {
                ProductName = "Samsung Galaxy S24 Ultra",
                BrandName = "Samsung",
                Color = "Titanium Black",
                StorageCapacity = 512,
                Processor = "Snapdragon 8 Gen 3",
                ScreenSize = 6.8m,
                BatteryCapacity = 5000,
                ImageUrl = "https://example.com/s24ultra.jpg",
                ImageGalleryJson = "[\"s24ultra1.jpg\",\"s24ultra2.jpg\"]",
                CostPrice = 23000000,
                SellPrice = 25990000,
                StockQuantity = 15,
                Description = "Camera 200MP, hỗ trợ bút S-Pen",
                IsDraft = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            },
            new Product
            {
                ProductName = "Xiaomi 14T",
                BrandName = "Xiaomi",
                Color = "Mint Green",
                StorageCapacity = 256,
                Processor = "Dimensity 8300-Ultra",
                ScreenSize = 6.67m,
                BatteryCapacity = 5000,
                ImageUrl = "https://example.com/xiaomi14t.jpg",
                ImageGalleryJson = "[\"xiaomi14t1.jpg\",\"xiaomi14t2.jpg\"]",
                CostPrice = 12000000,
                SellPrice = 14990000,
                StockQuantity = 20,
                Description = "Cấu hình mạnh, giá dễ tiếp cận",
                IsDraft = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        Console.WriteLine("Seed data for Products created successfully!");
    }
}