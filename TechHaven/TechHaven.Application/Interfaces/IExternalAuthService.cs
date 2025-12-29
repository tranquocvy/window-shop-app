using TechHaven.Domain.Entities;

namespace TechHaven.Application.Interfaces;

public interface IExternalAuthService
{
    /// <summary>
    /// Thử kết nối và lấy thông tin User từ Database bên ngoài.
    /// </summary>
    Task<User?> GetUserFromExternalDbAsync(string connectionString, string userName);

    /// <summary>
    /// Kiểm tra xem kết nối có hợp lệ không.
    /// </summary>
    Task<bool> TestConnectionAsync(string connectionString);

    // Thêm hàm này: Application nhờ Infrastructure build giúp chuỗi kết nối chuẩn
    string BuildConnectionString(string host, string port, string dbName, string dbUser, string dbPass);

    /// <summary>
    /// [MỚI] Tìm User theo ID từ DB External (Dùng cho Verify OTP)
    /// </summary>
    Task<User?> GetUserByIdFromExternalDbAsync(string connectionString, int userId);

    /// <summary>
    /// [MỚI] Cập nhật Refresh Token vào DB External
    /// </summary>
    Task UpdateRefreshTokenAsync(string connectionString, int userId, string refreshToken, DateTime expiryTime);
   /// <summary>
   /// Tìm User theo Refresh Token từ DB External
   /// </summary>
   /// <param name="connectionString"></param>
   /// <param name="refreshToken"></param>
   /// <returns></returns>
    Task<User?> GetUserByRefreshTokenFromExternalDbAsync(string connectionString, string refreshToken);
}