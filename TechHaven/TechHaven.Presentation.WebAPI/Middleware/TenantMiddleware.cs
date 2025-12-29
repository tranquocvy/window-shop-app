using System.Security.Claims;
using TechHaven.Infrastructure.Services;
using TechHaven.Application.Interfaces; // Chứa IStringEncryptionHelper

namespace TechHaven.Presentation.WebAPI.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantService tenantService,
        IStringEncryptionHelper encryptionHelper)
    {
        // 1. Kiểm tra xem User đã được Authentication Middleware xác thực chưa
        if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
        {
            // 2. Tìm claim "db_config" trong token
            var dbConfigClaim = context.User.Claims.FirstOrDefault(c => c.Type == "db_config");

            if (dbConfigClaim != null && !string.IsNullOrEmpty(dbConfigClaim.Value))
            {
                try
                {
                    // 3. Giải mã Connection String
                    var connectionString = encryptionHelper.Decrypt(dbConfigClaim.Value);

                    // 4. Set vào TenantService (Scoped Service)
                    // Vì TenantService là Scoped, nó sẽ sống suốt vòng đời request này
                    // Khi AppDbContext được tạo ra sau đó, nó sẽ đọc được giá trị này.
                    tenantService.SetConnectionString(connectionString);

                    _logger.LogDebug("Switched to external database for user {User}", context.User.Identity.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt database configuration from token.");
                    // Nếu lỗi, không làm gì cả -> AppDbContext sẽ dùng Default DB
                    // Hoặc có thể return 401 nếu muốn chặt chẽ
                    throw new Exception("Error occurred during tenant database decryption.");
                }
            }
        }

        // 5. Đi tiếp tới Controller
        await _next(context);
    }
}