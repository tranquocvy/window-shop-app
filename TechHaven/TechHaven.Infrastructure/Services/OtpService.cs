using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services;

public class OtpService : IOtpService
{
    // Store: SessionId -> (UserId, OtpCode, ExpiryTime)
    private readonly ConcurrentDictionary<string, (int UserId, string OtpCode, DateTime ExpiryTime)> _otpStore = new();
    private const int OtpExpirationMinutes = 5;

    public (string OtpSessionId, string OtpCode) GenerateOtp(int userId)
    {
        // OLD: Dễ đoán mã OTP nếu như có mã nguồn
        // Generate 6-digit OTP
        // var otpCode = new Random().Next(100000, 999999).ToString();
        
        // NEW: SỬ DỤNG Cryptographically Secure Random
        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        // Generate session ID (GUID)
        var sessionId = Guid.NewGuid().ToString();

        // Store OTP with expiry time
        var expiryTime = DateTime.UtcNow.AddMinutes(OtpExpirationMinutes);
        _otpStore[sessionId] = (userId, otpCode, expiryTime);

        // Clean up expired OTPs
        CleanupExpiredOtps();

        return (sessionId, otpCode);
    }

    public int? ValidateOtp(string otpSessionId, string otpCode)
    {
        if (!_otpStore.TryGetValue(otpSessionId, out var storedOtp))
        {
            return null; // Session not found
        }

        // Check if OTP is expired
        if (DateTime.UtcNow > storedOtp.ExpiryTime)
        {
            _otpStore.TryRemove(otpSessionId, out _);
            return null;
        }

        // Check if OTP code matches
        if (storedOtp.OtpCode != otpCode)
        {
            return null;
        }

        return storedOtp.UserId;
    }

    public void InvalidateOtp(string otpSessionId)
    {
        _otpStore.TryRemove(otpSessionId, out _);
    }

    public int? GetUserIdFromSession(string otpSessionId)
    {
        if (!_otpStore.TryGetValue(otpSessionId, out var storedOtp))
        {
            return null; // Session not found
        }

        // Check if OTP is expired
        if (DateTime.UtcNow > storedOtp.ExpiryTime)
        {
            _otpStore.TryRemove(otpSessionId, out _);
            return null;
        }

        return storedOtp.UserId;
    }

    private void CleanupExpiredOtps()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _otpStore
            .Where(kvp => kvp.Value.ExpiryTime < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _otpStore.TryRemove(key, out _);
        }
    }
}
