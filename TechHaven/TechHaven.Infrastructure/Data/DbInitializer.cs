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
    }
}