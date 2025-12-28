using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Infrastructure.Persistence;

namespace TechHaven.Infrastructure.Services;

public class ExternalAuthService : IExternalAuthService
{
    private readonly IConfiguration _configuration;

    public ExternalAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Helper tạo Context thủ công
    private AppDbContext CreateDynamicContext(string connectionString)
    {
        // 1. Tạo Builder
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // 2. Cấu hình Connection String trực tiếp
        optionsBuilder.UseNpgsql(connectionString);
        // [FIX QUAN TRỌNG] Bổ sung dòng này để EF Core hiểu User class -> users table
        optionsBuilder.UseSnakeCaseNamingConvention();
        // 3. Khởi tạo Context bằng Constructor chuẩn (1 tham số)
        // KHÔNG CẦN config hay tenantService nữa vì options đã chứa thông tin kết nối rồi. [Bản chất config và tenantService là cấu tạo chuỗi kết nối, khi đã có chuỗi đó rồi thì ko cần]
        return new AppDbContext(optionsBuilder.Options); //Lưu ý: Cân nhắc thêm tenantService
    }

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        using var context = CreateDynamicContext(connectionString);
        return await context.Database.CanConnectAsync();
    }

    public async Task<User?> GetUserFromExternalDbAsync(string connectionString, string userName)
    {
        using var context = CreateDynamicContext(connectionString);

        // Vì đây là thao tác đọc 1 lần (Read-Only) để login, ta dùng AsNoTracking cho nhẹ
        // Include Role là bắt buộc để logic tạo Token sau này hoạt động
        return await context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }
    public string BuildConnectionString(string host, string port, string dbName, string dbUser, string dbPass)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(port, out int p) ? p : 5432,
            Database = dbName,
            Username = dbUser,
            Password = dbPass,

            // Cấu hình chuẩn cho Supabase/Cloud
            SslMode = SslMode.Require,
            //TrustServerCertificate = true,
            Pooling = true,
            IncludeErrorDetail = true
        };

        return builder.ToString();
    }

    public async Task<User?> GetUserByIdFromExternalDbAsync(string connectionString, int userId)
    {
        // Tạo context kết nối tới DB External
        using var context = CreateDynamicContext(connectionString);

        // Query tìm User theo ID
        // Quan trọng: Phải Include Role để sau này tạo JWT Token không bị lỗi thiếu Claim Role
        return await context.Users
            .AsNoTracking() // Dùng AsNoTracking cho nhẹ vì chỉ đọc
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task UpdateRefreshTokenAsync(string connectionString, int userId, string refreshToken, DateTime expiryTime)
    {
        // Tạo context kết nối tới DB External
        using var context = CreateDynamicContext(connectionString);

        // Bước 1: Tìm user cần update (Phải tracking để EF Core biết có thay đổi)
        var user = await context.Users.FindAsync(userId);

        if (user != null)
        {
            // Bước 2: Cập nhật thông tin token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;

            // Cập nhật LastLogin luôn nếu muốn đồng bộ logic với flow chính
            user.LastLoginAt = DateTime.UtcNow;

            // Bước 3: Lưu xuống DB External
            await context.SaveChangesAsync();
        }
        // Lưu ý: Nếu user null (rất hiếm khi xảy ra ở bước này vì đã verify ID), 
        // ta có thể bỏ qua hoặc throw exception tùy nhu cầu, ở đây tôi chọn bỏ qua cho an toàn.
    }
}