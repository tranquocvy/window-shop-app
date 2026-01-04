using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services;

public class OtpService : IOtpService
{
    // Store: SessionId -> (UserId, OtpCode, ExpiryTime)
    private readonly ConcurrentDictionary<string, (int UserId, string OtpCode, DateTime ExpiryTime)> _otpStore = new();
    private readonly ILogger<OtpService> _logger;
    private const int OtpExpirationMinutes = 5;
    private readonly Timer _cleanupTimer;

    public OtpService(ILogger<OtpService> logger)
    {
        _logger = logger;
        // Cleanup every 5 minutes
        _cleanupTimer = new Timer(
            CleanupExpiredOtps,
            null,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5)
        );
    }

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

        _logger.LogInformation(
            "Generated OTP session {SessionId} for user {UserId}. Expires at {Expiry}",
            sessionId,
            userId,
            expiryTime);

        // Clean up expired OTPs
        CleanupExpiredOtps(null);

        return (sessionId, otpCode);
    }

    public int? ValidateOtp(string otpSessionId, string otpCode)
    {
        if (!_otpStore.TryGetValue(otpSessionId, out var storedOtp))
        {
            _logger.LogWarning(
                "OTP validation failed. Session not found: {SessionId}",
                otpSessionId);
            return null; // Session not found
        }

        // Check if OTP is expired
        if (DateTime.UtcNow > storedOtp.ExpiryTime)
        {
            _otpStore.TryRemove(otpSessionId, out _);
            _logger.LogWarning(
                "OTP session expired: {SessionId}",
                otpSessionId);
            return null;
        }

        // Check if OTP code matches
        if (storedOtp.OtpCode != otpCode)
        {
            _logger.LogWarning(
                "OTP validation failed. Session: {SessionId}",
                otpSessionId);
            return null;
        }

        _logger.LogInformation(
            "OTP validated for session {SessionId}",
            otpSessionId);

        return storedOtp.UserId;
    }

    public void InvalidateOtp(string otpSessionId)
    {
        if (_otpStore.TryRemove(otpSessionId, out _))
        {
            _logger.LogInformation(
                "OTP session invalidated: {SessionId}",
                otpSessionId);
        }
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

    private void CleanupExpiredOtps(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _otpStore
                .Where(kvp => kvp.Value.ExpiryTime < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                if (_otpStore.TryRemove(key, out _))
                {
                    _logger.LogDebug("Cleaned up expired OTP: {SessionId}", key);
                }
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired OTP entries", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OTP cleanup");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
