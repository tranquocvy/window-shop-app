using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services;

public class OtpService : IOtpService
{
    // Store: SessionId -> (UserId, OtpCode, ExpiryTime)
    private readonly ConcurrentDictionary<string, (int UserId, string OtpCode, DateTime ExpiryTime)> _otpStore = new();
    private const int OtpExpirationMinutes = 5;

    public string GenerateOtp(int userId)
    {
        // Generate 6-digit OTP
        var otpCode = new Random().Next(100000, 999999).ToString();

        // Generate session ID (GUID)
        var sessionId = Guid.NewGuid().ToString();

        // Store OTP with expiry time
        var expiryTime = DateTime.UtcNow.AddMinutes(OtpExpirationMinutes);
        _otpStore[sessionId] = (userId, otpCode, expiryTime);

        // Clean up expired OTPs
        CleanupExpiredOtps();

        return sessionId;
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
