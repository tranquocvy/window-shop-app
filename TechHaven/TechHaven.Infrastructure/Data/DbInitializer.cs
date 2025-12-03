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

        // Products are seeded via CellphoneProductSeeder (WebAPI project) to ensure
        // image assets are uploaded to Supabase during the import process.

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