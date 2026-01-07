using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.RefreshTokenExternal;
using TechHaven.Domain.Entities;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.External;

[ExcludeFromCodeCoverage]
public class RefreshTokenExternalCommandHandlerTests : UnitTestBase
{
    private readonly RefreshTokenExternalCommandHandler _handler;

    public RefreshTokenExternalCommandHandlerTests()
    {
        _handler = new RefreshTokenExternalCommandHandler(
            MockExternalAuthService.Object, // Từ UnitTestBase
            MockEncryptionHelper.Object,    // Từ UnitTestBase
            MockJwtTokenService.Object      // Từ UnitTestBase
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldRotateTokens_And_UpdateExternalDb()
    {
        // --- ARRANGE ---
        var refreshToken = "valid_refresh_token";
        var encryptedConfig = "encrypted_db_config";
        var decryptedConnString = "Server=remote;Database=tenant1;...";

        var command = new RefreshTokenExternalCommand(refreshToken, encryptedConfig);

        var user = new User
        {
            UserId = 10,
            UserName = "tenant_user",
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1) // Còn hạn
        };

        // 1. Mock Decrypt Config
        MockEncryptionHelper.Setup(x => x.Decrypt(encryptedConfig))
            .Returns(decryptedConnString);

        // 2. Mock Get User from External DB
        MockExternalAuthService.Setup(x => x.GetUserByRefreshTokenFromExternalDbAsync(decryptedConnString, refreshToken))
            .ReturnsAsync(user);

        // 3. Mock Generate Tokens
        // QUAN TRỌNG: Verify AccessToken mới được tạo kèm theo EncryptedDbConfig
        MockJwtTokenService.Setup(x => x.GenerateAccessToken(user, encryptedConfig))
            .Returns("new_access_token");
        MockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("new_refresh_token");

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
        result.NewRefreshToken.Should().Be("new_refresh_token");

        // Verify: Phải update token mới vào DB External
        MockExternalAuthService.Verify(x => x.UpdateRefreshTokenAsync(
            decryptedConnString,
            user.UserId,
            "new_refresh_token",
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DecryptionFailed_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new RefreshTokenExternalCommand("token", "bad_config");

        // Mock Decrypt -> Throw Exception
        MockEncryptionHelper.Setup(x => x.Decrypt("bad_config"))
            .Throws(new Exception("Decryption error"));

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        // Check key "EncryptedDbConfig" như trong catch block của handler
        ex.Errors.Should().ContainKey("EncryptedDbConfig");

        // Verify: Không gọi DB
        MockExternalAuthService.Verify(x => x.GetUserByRefreshTokenFromExternalDbAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TokenNotFound_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new RefreshTokenExternalCommand("unknown_token", "config");
        var connString = "conn";

        MockEncryptionHelper.Setup(x => x.Decrypt("config")).Returns(connString);

        // Mock Get User -> NULL (Token sai hoặc không tồn tại)
        MockExternalAuthService.Setup(x => x.GetUserByRefreshTokenFromExternalDbAsync(connString, "unknown_token"))
            .ReturnsAsync((User?)null);

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Errors.Should().ContainKey("RefreshToken");
        ex.Errors["RefreshToken"].Should().Contain("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_TokenExpired_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new RefreshTokenExternalCommand("expired_token", "config");
        var connString = "conn";

        var user = new User
        {
            RefreshToken = "expired_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1) // Đã hết hạn
        };

        MockEncryptionHelper.Setup(x => x.Decrypt("config")).Returns(connString);

        MockExternalAuthService.Setup(x => x.GetUserByRefreshTokenFromExternalDbAsync(connString, "expired_token"))
            .ReturnsAsync(user);

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Errors.Should().ContainKey("RefreshToken");
        ex.Errors["RefreshToken"].Should().Contain("Refresh token has expired");

        // Verify: Không được tạo token mới
        MockJwtTokenService.Verify(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }
}