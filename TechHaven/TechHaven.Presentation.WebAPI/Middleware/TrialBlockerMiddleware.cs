using System.Security.Claims;

namespace TechHaven.Presentation.WebAPI.Middleware;

public class TrialBlockerMiddleware
{
    private readonly RequestDelegate _next;

    // Danh sách các API được phép đi qua (WhiteList)
    // Lưu ý: Viết chữ thường toàn bộ để so sánh không phân biệt hoa thường
    private readonly string[] _allowList = new[]
    {
        "/api/auth/login",
        "/api/auth/verify-otp",
        "/api/auth/resend-otp",
        "/api/auth/refresh-token",
        "/api/auth/me",
        "/api/auth/forgot-password",
        "/api/auth/reset-password",
        "/api/auth/isactive",
        "/api/auth/activate",
        "/api/auth/login-external",
        "/api/auth/refresh-token-external",
        "/api/auth/resend-otp-external", 
        "/api/auth/verify-otp-external",
       //"/api/auth/signup" // ko mở sign up là do chỉ có Admin mới được phép thêm

    };

    public TrialBlockerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Lấy đường dẫn hiện tại (URL Path)
        var path = context.Request.Path.Value?.ToLower().TrimEnd('/');
        var method = context.Request.Method; // Lấy method: "GET", "POST", "PUT"...

        // 2. Kiểm tra Allowlist
        // - Là API nằm trong AllowList (các API Auth POST)
        // - HOẶC: Là method GET (Cho phép xem dữ liệu thoải mái)
        bool isWhitelistedPath = path != null && _allowList.Any(p => path.StartsWith(p));
        bool isGetMethod = HttpMethods.IsGet(method); // Kiểm tra xem có phải GET không

        if (isWhitelistedPath || isGetMethod)
        {
            await _next(context);
            return;
        }

        // 3. Kiểm tra User có đăng nhập không?
        // Nếu chưa đăng nhập (Anonymous) thì cho qua (để Auth Middleware bên dưới xử lý 401 sau)
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        // 4. Lấy thông tin từ Claims (Đọc từ Token, siêu nhanh, không cần query DB)
        var createdAtClaim = context.User.Claims.FirstOrDefault(c => c.Type == "created_at");
        var isActiveClaim = context.User.Claims.FirstOrDefault(c => c.Type == "is_active");

        if (createdAtClaim != null && isActiveClaim != null)
        {
            // Parse dữ liệu
            bool isActive = bool.Parse(isActiveClaim.Value);
            DateTime createdAt = DateTime.Parse(createdAtClaim.Value).ToUniversalTime();

            // 5. LOGIC CHẶN CHÍNH: !Active && >= 15 ngày
            if (!isActive && (DateTime.UtcNow - createdAt).TotalDays >= 15)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Message = "Trial period expired. Please upgrade your account to continue using this feature.",
                    ErrorCode = "TRIAL_EXPIRED"
                });

                return; // CẮT LUỒNG NGAY LẬP TỨC
            }
        }

        // 6. Nếu thỏa mãn hết -> Cho đi tiếp
        await _next(context);
    }
}