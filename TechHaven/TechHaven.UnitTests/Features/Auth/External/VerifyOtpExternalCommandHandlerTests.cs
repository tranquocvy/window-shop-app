using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.VerifyOtpExternal;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.External;

[ExcludeFromCodeCoverage]
public class VerifyOtpExternalCommandHandlerTests : UnitTestBase
{
    private readonly VerifyOtpExternalCommandHandler _handler;

    public VerifyOtpExternalCommandHandlerTests()
    {

        _handler = new VerifyOtpExternalCommandHandler(
            MockOtpService.Object,          // Từ UnitTestBase
            MockExternalAuthService.Object, // Từ UnitTestBase
            MockEncryptionHelper.Object,    // Từ UnitTestBase
            MockJwtTokenService.Object
        );
    }

    [Fact]
    public async Task Handle_ValidOtp_ShouldDecryptConfig_GenerateTokens_And_SaveToExternalDb()
    {
        // --- ARRANGE ---
        var sessionId = Guid.NewGuid().ToString();
        var otpCode = "123456";
        var encryptedConfig = "encrypted_db_config";
        var decryptedConnString = "Server=remote_ip;Database=tenant_db;...";

        var command = new VerifyOtpExternalCommand(sessionId, otpCode, encryptedConfig);

        var user = new User
        {
            UserId = 10,
            UserName = "tenant_admin",
            UserFullName = "Tenant Admin",
            RoleId = 2,
            Role = new Role { RoleId = 2, RoleName = "Manager" }
        };

        // 1. Mock Validate OTP -> Success
        MockOtpService.Setup(x => x.ValidateOtp(sessionId, otpCode)).Returns(10);

        // 2. Mock Decrypt Config
        MockEncryptionHelper.Setup(x => x.Decrypt(encryptedConfig)).Returns(decryptedConnString);

        // 3. Mock Get User from EXTERNAL DB
        MockExternalAuthService.Setup(x => x.GetUserByIdFromExternalDbAsync(decryptedConnString, 10))
            .ReturnsAsync(user);

        // 4. Mock Generate Tokens (Quan trọng: Phải truyền encryptedConfig vào AccessToken)
        MockJwtTokenService.Setup(x => x.GenerateAccessToken(user, encryptedConfig))
            .Returns("access_token_ext");
        MockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token_ext");

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token_ext");
        result.UserName.Should().Be("tenant_admin");

        // Verify Flow:
        // 1. Phải Invalidate OTP
        MockOtpService.Verify(x => x.InvalidateOtp(sessionId), Times.Once);

        // 2. Phải Update Refresh Token vào EXTERNAL DB (không phải UnitOfWork)
        MockExternalAuthService.Verify(x => x.UpdateRefreshTokenAsync(
            decryptedConnString,
            user.UserId,
            "refresh_token_ext",
            It.IsAny<DateTime>()), Times.Once);

        // 3. Đảm bảo KHÔNG gọi UnitOfWork (vì đây là External)
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidOtp_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new VerifyOtpExternalCommand("session", "wrong_code", "config");

        // Mock OTP Failed
        MockOtpService.Setup(x => x.ValidateOtp(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((int?)null);

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Errors.Should().ContainKey("OtpCode");

        // Verify: Không giải mã, không gọi DB
        MockEncryptionHelper.Verify(x => x.Decrypt(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFoundInExternalDb_ShouldThrowNotFoundException()
    {
        // --- ARRANGE ---
        var command = new VerifyOtpExternalCommand("session", "code", "config");
        var connString = "conn_string";

        MockOtpService.Setup(x => x.ValidateOtp(It.IsAny<string>(), It.IsAny<string>())).Returns(10);
        MockEncryptionHelper.Setup(x => x.Decrypt("config")).Returns(connString);

        // Mock Get User from External -> NULL
        MockExternalAuthService.Setup(x => x.GetUserByIdFromExternalDbAsync(connString, 10))
            .ReturnsAsync((User?)null);

        // --- ACT & ASSERT ---
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}