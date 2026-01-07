using FluentAssertions;
//using FluentValidation; // Dùng cho ValidationException
using FluentValidation.Results;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.LoginExternal;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.External;

[ExcludeFromCodeCoverage]
public class LoginExternalCommandHandlerTests : UnitTestBase
{
    private readonly LoginExternalCommandHandler _handler;


    public LoginExternalCommandHandlerTests()
    {

        _handler = new LoginExternalCommandHandler(
            MockHasher.Object,
            MockExternalAuthService.Object,
            MockEncryptionHelper.Object,
            MockOtpService.Object,
            MockEmailService.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnEncryptedConfig_AndSendEmail()
    {
        // --- ARRANGE ---
        var command = new LoginExternalCommand("localhost", "5432", "mydb", "user", "pass", "admin", "password123");
        var fakeConnString = "Server=localhost;User Id=user;...";
        var encryptedConfig = "encrypted_string_base64";

        var user = new User
        {
            UserId = 100,
            UserName = "admin",
            UserFullName = "External User",
            Email = "ext@techhaven.com",
            PasswordHash = "hashed_pass",
            CreatedAt = DateTime.UtcNow
        };

        // 1. Mock Build Connection String
        MockExternalAuthService.Setup(x => x.BuildConnectionString(
            command.DbHost, command.DbPort, command.DbName, command.DbUser, command.DbPass))
            .Returns(fakeConnString);

        // 2. Mock Test Connection -> True
        MockExternalAuthService.Setup(x => x.TestConnectionAsync(fakeConnString))
            .ReturnsAsync(true);

        // 3. Mock Get User -> Found
        MockExternalAuthService.Setup(x => x.GetUserFromExternalDbAsync(fakeConnString, "admin"))
            .ReturnsAsync(user);

        // 4. Mock Verify Pass -> True
        MockHasher.Setup(x => x.VerifyPassword("password123", "hashed_pass"))
            .Returns(true);

        // 5. Mock OTP & Encrypt
        MockOtpService.Setup(x => x.GenerateOtp(user.UserId)).Returns(("session_ext", "999999"));
        MockEncryptionHelper.Setup(x => x.Encrypt(fakeConnString)).Returns(encryptedConfig);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.OtpSessionId.Should().Be("session_ext");

        // Kiểm tra quan trọng nhất: Chuỗi kết nối phải được mã hóa trả về
        result.EncryptedDbConfig.Should().Be(encryptedConfig);
        result.RequiresOtp.Should().BeTrue();

        // Verify Email gửi đi
        MockEmailService.Verify(x => x.SendOtpEmailAsync(
            user.Email, user.UserFullName, "999999", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConnectionFailed_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new LoginExternalCommand("bad_host", "5432", "db", "u", "p", "admin", "pass");
        var fakeConnString = "BadConnection";

        MockExternalAuthService.Setup(x => x.BuildConnectionString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fakeConnString);

        // Mock Test Connection -> FALSE
        MockExternalAuthService.Setup(x => x.TestConnectionAsync(fakeConnString))
            .ReturnsAsync(false);

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        // Kiểm tra đúng lỗi Validation về Database
        ex.Errors.Should().Contain(x => x.Key == "Database");
    }

    [Fact]
    public async Task Handle_UserNotFoundInExternalDb_ShouldThrowNotFoundException()
    {
        // --- ARRANGE ---
        var command = new LoginExternalCommand("host", "port", "db", "u", "p", "ghost", "pass");
        var fakeConnString = "GoodConn";

        MockExternalAuthService.Setup(x => x.BuildConnectionString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fakeConnString);
        MockExternalAuthService.Setup(x => x.TestConnectionAsync(fakeConnString)).ReturnsAsync(true);

        // Mock Get User -> NULL
        MockExternalAuthService.Setup(x => x.GetUserFromExternalDbAsync(fakeConnString, "ghost"))
            .ReturnsAsync((User?)null);

        // --- ACT & ASSERT ---
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_EmailServiceFails_ShouldRollbackOtp()
    {
        // --- ARRANGE ---
        var command = new LoginExternalCommand("h", "p", "d", "u", "p", "admin", "pass");
        var user = new User { UserId = 1, UserName = "admin", Email = "a@b.com", PasswordHash = "hash" };
        var sessionId = "session_rollback";

        // Setup Happy Path tới đoạn gửi mail
        MockExternalAuthService.Setup(x => x.BuildConnectionString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("conn");
        MockExternalAuthService.Setup(x => x.TestConnectionAsync("conn")).ReturnsAsync(true);
        MockExternalAuthService.Setup(x => x.GetUserFromExternalDbAsync("conn", "admin")).ReturnsAsync(user);
        MockHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        MockOtpService.Setup(x => x.GenerateOtp(user.UserId)).Returns((sessionId, "123"));

        // Gửi mail thất bại
        MockEmailService.Setup(x => x.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP Error"));

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));

        // Verify Rollback
        MockOtpService.Verify(x => x.InvalidateOtp(sessionId), Times.Once);
    }
}