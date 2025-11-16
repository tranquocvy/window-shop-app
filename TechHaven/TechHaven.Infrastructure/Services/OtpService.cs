using Microsoft.Extensions.Caching.Memory;
using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private const int OTP_EXPIRY_MINUTES = 5;
    private const int OTP_LENGTH = 6;

    public OtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateOtp(int userId)
    {
        // Generate 6-digit random OTP
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        // Store in cache with 5 minutes expiry
        var cacheKey = GetCacheKey(userId);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OTP_EXPIRY_MINUTES)
        };

        _cache.Set(cacheKey, otpCode, cacheOptions);

        return otpCode;
    }

    public bool ValidateOtp(int userId, string otpCode)
    {
        var cacheKey = GetCacheKey(userId);

        if (_cache.TryGetValue(cacheKey, out string? storedOtp))
        {
            return storedOtp == otpCode;
        }

        return false;
    }

    public void InvalidateOtp(int userId)
    {
        var cacheKey = GetCacheKey(userId);
        _cache.Remove(cacheKey);
    }

    private static string GetCacheKey(int userId) => $"OTP_{userId}";
}
