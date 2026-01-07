using FluentAssertions;
using FluentValidation.Results; // Để dùng ValidationFailure nếu cần check sâu
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.ResendOtpExternal;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.External;

[ExcludeFromCodeCoverage]
public class ResendOtpExternalCommandHandlerTests : UnitTestBase
{
    private readonly ResendOtpExternalCommandHandler _handler;

    public ResendOtpExternalCommandHandlerTests()
    {
        _handler = new ResendOtpExternalCommandHandler(
            MockExternalAuthService.Object, // Từ UnitTestBase
            MockOtpService.Object,          // Từ UnitTestBase
            MockEmailService.Object,        // Từ UnitTestBase
            MockEncryptionHelper.Object     // Từ UnitTestBase
        );
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldDecrypt_InvalidateOld_And_SendNewOtp()
    {
        // --- ARRANGE ---
        var oldSession = Guid.NewGuid().ToString();
        var newSession = Guid.NewGuid().ToString();
        var encryptedConfig = "encrypted_config";
        var connString = "Server=remote;...";
        var command = new ResendOtpExternalCommand(oldSession, encryptedConfig);

        var user = new User { UserId = 10, Email = "ext@test.com", UserFullName = "Ext User" };

        // 1. Mock Check Session -> OK
        MockOtpService.Setup(x => x.GetUserIdFromSession(oldSession)).Returns(user.UserId);

        // 2. Mock Decrypt -> OK
        MockEncryptionHelper.Setup(x => x.Decrypt(encryptedConfig)).Returns(connString);

        // 3. Mock Get User External -> Found
        MockExternalAuthService.Setup(x => x.GetUserByIdFromExternalDbAsync(connString, user.UserId))
            .ReturnsAsync(user);

        // 4. Mock Generate New OTP
        MockOtpService.Setup(x => x.GenerateOtp(user.UserId)).Returns((newSession, "654321"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.IsOtpResent.Should().BeTrue();
        result.NewOtpSessionId.Should().Be(newSession);

        // Verify Flow:
        // Giải mã config
        MockEncryptionHelper.Verify(x => x.Decrypt(encryptedConfig), Times.Once);

        // Hủy session cũ
        MockOtpService.Verify(x => x.InvalidateOtp(oldSession), Times.Once);

        // Gửi mail mới
        MockEmailService.Verify(x => x.SendOtpEmailAsync(
            user.Email, user.UserFullName, "654321", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidSession_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new ResendOtpExternalCommand("invalid_session", "config");

        // Mock Session check -> NULL
        MockOtpService.Setup(x => x.GetUserIdFromSession(It.IsAny<string>()))
            .Returns((int?)null);

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Errors.Should().ContainKey("OtpSessionId");

        // Verify: Không được gọi Decrypt hay DB
        MockEncryptionHelper.Verify(x => x.Decrypt(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DecryptionFailed_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new ResendOtpExternalCommand("session", "bad_config");

        // Mock Session OK
        MockOtpService.Setup(x => x.GetUserIdFromSession("session")).Returns(1);

        // Mock Decrypt -> Throw Exception (Mô phỏng config sai/tampered)
        MockEncryptionHelper.Setup(x => x.Decrypt("bad_config"))
            .Throws(new Exception("Decryption error"));

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        // Kiểm tra catch block trong Handler: throw new ValidationException(..."Config"...)
        ex.Errors.Should().ContainKey("Config");

        // Verify: Không được gọi DB External
        MockExternalAuthService.Verify(x => x.GetUserByIdFromExternalDbAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFoundInExternalDb_ShouldThrowNotFoundException()
    {
        // --- ARRANGE ---
        var command = new ResendOtpExternalCommand("session", "config");
        var connString = "conn";

        MockOtpService.Setup(x => x.GetUserIdFromSession("session")).Returns(99);
        MockEncryptionHelper.Setup(x => x.Decrypt("config")).Returns(connString);

        // Mock Get User -> NULL
        MockExternalAuthService.Setup(x => x.GetUserByIdFromExternalDbAsync(connString, 99))
            .ReturnsAsync((User?)null);

        // --- ACT & ASSERT ---
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}