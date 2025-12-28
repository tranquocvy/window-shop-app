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
                    UserFullName = "Nguyễn Phúc Hoàng",
                    Email = "nphuchoang.itus@gmail.com",
                    UserName = "nphoang_seller",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 2,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
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
                    UserFullName = "Nguyễn Văn Bình Dương",
                    Email = "dn016777@gmail.com",
                    UserName = "nvbduong_seller",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456", workFactor: 12),
                    RoleId = 2,
                    IsActive = true,
                    HasSeenGuide = false,
                    CreatedAt = DateTime.UtcNow,
                    ActivatedAt = DateTime.UtcNow
                },
                new User
                {
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

        // 3. Seed System AppSettings (UserId = null)
        if (!await context.AppSettings.AnyAsync(s => s.UserId == null))
        {
            var systemSettings = new List<AppSetting>
            {
                // Store Information
                new AppSetting
                {
                    Key = "Store.Name",
                    Value = "TechHaven Electronics",
                    ValueType = SettingType.String,
                    Category = "Store",
                    Description = "Store name displayed on receipts and reports",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Store.Address",
                    Value = "123 Nguyen Hue St, District 1, Ho Chi Minh City",
                    ValueType = SettingType.String,
                    Category = "Store",
                    Description = "Store physical address",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Store.Phone",
                    Value = "028-1234-5678",
                    ValueType = SettingType.String,
                    Category = "Store",
                    Description = "Store contact phone number",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Store.Email",
                    Value = "contact@techhaven.vn",
                    ValueType = SettingType.String,
                    Category = "Store",
                    Description = "Store contact email",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                
                // POS Settings
                new AppSetting
                {
                    Key = "POS.AutoPrint",
                    Value = "true",
                    ValueType = SettingType.Bool,
                    Category = "POS",
                    Description = "Automatically print receipt after order completion",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "POS.DefaultDiscountRate",
                    Value = "0.05",
                    ValueType = SettingType.Decimal,
                    Category = "POS",
                    Description = "Default discount rate for VIP customers (5%)",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "POS.StudentDiscountRate",
                    Value = "0.10",
                    ValueType = SettingType.Decimal,
                    Category = "POS",
                    Description = "Discount rate for student customers (10%)",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "POS.LowStockThreshold",
                    Value = "10",
                    ValueType = SettingType.Number,
                    Category = "POS",
                    Description = "Alert when stock quantity falls below this threshold",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                
                // Commission Settings
                new AppSetting
                {
                    Key = "Commission.BaseRate",
                    Value = "0.02",
                    ValueType = SettingType.Decimal,
                    Category = "Commission",
                    Description = "Base commission rate (2%)",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Commission.TierOneThreshold",
                    Value = "50000000",
                    ValueType = SettingType.Decimal,
                    Category = "Commission",
                    Description = "Tier 1 sales threshold (50M VND)",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Commission.TierOneRate",
                    Value = "0.03",
                    ValueType = SettingType.Decimal,
                    Category = "Commission",
                    Description = "Tier 1 commission rate (3%)",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Commission.TierTwoThreshold",
                    Value = "100000000",
                    ValueType = SettingType.Decimal,
                    Category = "Commission",
                    Description = "Tier 2 sales threshold (100M VND)",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Commission.TierTwoRate",
                    Value = "0.05",
                    ValueType = SettingType.Decimal,
                    Category = "Commission",
                    Description = "Tier 2 commission rate (5%)",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                
                // Report Settings
                new AppSetting
                {
                    Key = "Report.DefaultDateRange",
                    Value = "30",
                    ValueType = SettingType.Number,
                    Category = "Report",
                    Description = "Default date range for reports in days",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Report.TopProductsCount",
                    Value = "10",
                    ValueType = SettingType.Number,
                    Category = "Report",
                    Description = "Number of top products to show in reports",
                    IsSystem = true,
                    UserId = null,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.AppSettings.AddRangeAsync(systemSettings);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} system app settings.", systemSettings.Count);
        }

        // 4. Seed User-specific AppSettings for first seller user
        var firstSellerUser = await context.Users
            .FirstOrDefaultAsync(u => u.UserName == "nphoang_seller");

        if (firstSellerUser != null &&
            !await context.AppSettings.AnyAsync(s => s.UserId == firstSellerUser.UserId))
        {
            var userSettings = new List<AppSetting>
            {
                new AppSetting
                {
                    Key = "POS.AutoPrint",
                    Value = "false",
                    ValueType = SettingType.Bool,
                    Category = "POS",
                    Description = "User override: Don't auto-print receipts",
                    IsSystem = false,
                    UserId = firstSellerUser.UserId,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "UI.Theme",
                    Value = "Dark",
                    ValueType = SettingType.String,
                    Category = "UI",
                    Description = "User preferred UI theme",
                    IsSystem = false,
                    UserId = firstSellerUser.UserId,
                    UpdatedAt = DateTime.UtcNow
                },
                new AppSetting
                {
                    Key = "Notification.Email",
                    Value = "true",
                    ValueType = SettingType.Bool,
                    Category = "Notification",
                    Description = "Enable email notifications",
                    IsSystem = false,
                    UserId = firstSellerUser.UserId,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.AppSettings.AddRangeAsync(userSettings);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Seeded {Count} user-specific app settings for {UserName}.",
                userSettings.Count,
                firstSellerUser.UserName);
        }

        // 5. Seed Commissions for Seller users (last 6 months)
        var sellerUsers = await context.Users
            .Where(u => u.RoleId == 2) // Seller role
            .ToListAsync();

        if (sellerUsers.Any() && !await context.Commissions.AnyAsync())
        {
            var commissions = new List<Commission>();
            var currentDate = DateTime.UtcNow;
            var random = new Random(42); // Fixed seed for reproducible data

            foreach (var seller in sellerUsers)
            {
                // Generate commission data for last 6 months
                for (int monthOffset = 0; monthOffset < 6; monthOffset++)
                {
                    var targetDate = currentDate.AddMonths(-monthOffset);
                    var month = targetDate.Month;
                    var year = targetDate.Year;

                    // Generate random sales between 20M - 150M VND
                    var totalSales = (decimal)(random.NextDouble() * 130_000_000 + 20_000_000);

                    // Calculate commission rate based on tiers
                    decimal commissionRate;
                    if (totalSales >= 100_000_000)
                    {
                        commissionRate = 0.05m; // Tier 2: 5%
                    }
                    else if (totalSales >= 50_000_000)
                    {
                        commissionRate = 0.03m; // Tier 1: 3%
                    }
                    else
                    {
                        commissionRate = 0.02m; // Base: 2%
                    }

                    var commission = new Commission
                    {
                        UserId = seller.UserId,
                        Month = month,
                        Year = year,
                        TotalSales = Math.Round(totalSales, 2),
                        CommissionRate = commissionRate,
                        Note = monthOffset == 0
                            ? "Current month (in progress)"
                            : $"Month {month}/{year} performance",
                        CreatedAt = DateTime.UtcNow
                    };

                    commissions.Add(commission);
                }
            }

            await context.Commissions.AddRangeAsync(commissions);
            await context.SaveChangesAsync();
            logger.LogInformation(
                "Seeded {Count} commission records for {SellerCount} sellers.",
                commissions.Count,
                sellerUsers.Count);
        }

        // Products are seeded via CellphoneProductSeeder (WebAPI project) to ensure
        // image assets are uploaded to Supabase during the import process.

        // 6. Seed Customers
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

        logger.LogInformation("Database seeding completed successfully.");
    }
}