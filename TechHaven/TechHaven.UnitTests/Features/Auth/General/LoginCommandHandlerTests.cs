using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.Login;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class LoginCommandHandlerTests : UnitTestBase
{
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {

        _handler = new LoginCommandHandler(
            MockUow.Object,
            MockHasher.Object,
            MockOtpService.Object,
            MockEmailService.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldGenerateOtp_AndSendEmail()
    {
        // --- ARRANGE ---
        var command = new LoginCommand("admin", "password123");

        var user = new User
        {
            UserId = 1,
            UserName = "admin",
            Email = "admin@techhaven.com",
            UserFullName = "Admin User",
            PasswordHash = "hashed_secret"
        };

        // 1. Mock User tồn tại
        MockUserRepo.Setup(x => x.GetByUserNameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // 2. Mock Password đúng
        MockHasher.Setup(x => x.VerifyPassword("password123", "hashed_secret"))
            .Returns(true);

        // 3. Mock OTP Service trả về SessionId và Code
        MockOtpService.Setup(x => x.GenerateOtp(user.UserId))
            .Returns(("session_abc", "123456"));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.RequiresOtp.Should().BeTrue();
        result.OtpSessionId.Should().Be("session_abc");

        // Kiểm tra logic Mask Email (admin@techhaven.com -> ad***@t***.com)
        result.MaskedEmail.Should().StartWith("ad***@t");

        // Verify: Phải gửi email 1 lần
        MockEmailService.Verify(x => x.SendOtpEmailAsync(
            user.Email,
            user.UserFullName,
            "123456",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // --- ARRANGE ---
        var command = new LoginCommand("ghost", "password");

        MockUserRepo.Setup(x => x.GetByUserNameAsync("ghost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // --- ACT & ASSERT ---
        // Sử dụng NotFoundException như trong code (dù bảo mật thực tế nên dùng Generic Authentication Error)
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken));

        // Verify: Không được gọi OTP hay Email
        MockOtpService.Verify(x => x.GenerateOtp(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldThrowNotFoundException()
    {
        // --- ARRANGE ---
        var command = new LoginCommand("admin", "wrong_pass");
        var user = new User { UserName = "admin", PasswordHash = "real_hash" };

        MockUserRepo.Setup(x => x.GetByUserNameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Mock Hasher trả về false
        MockHasher.Setup(x => x.VerifyPassword("wrong_pass", "real_hash"))
            .Returns(false);

        // --- ACT & ASSERT ---
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken));
    }

    [Fact]
    public async Task Handle_EmailServiceFails_ShouldInvalidateOtp_AndThrowException()
    {
        // --- ARRANGE ---
        var command = new LoginCommand("admin", "password");
        var user = new User { UserId = 1, UserName = "admin", Email = "a@b.com", PasswordHash = "hash" };
        var sessionId = "session_fail";

        MockUserRepo.Setup(x => x.GetByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        MockHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // OTP sinh ra thành công
        MockOtpService.Setup(x => x.GenerateOtp(user.UserId)).Returns((sessionId, "123456"));

        // NHƯNG Gửi mail thất bại
        MockEmailService.Setup(x => x.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP Error"));

        // --- ACT & ASSERT ---
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken));
        exception.Message.Should().Contain("Failed to send OTP email");

        // CRITICAL CHECK: Đảm bảo OTP đã bị hủy (Rollback logic)
        MockOtpService.Verify(x => x.InvalidateOtp(sessionId), Times.Once);
    }

    [Theory]
    [InlineData("test@gmail.com", "te***@g***.com")]
    [InlineData("a@b.com", "***@b***.com")] // Case ngắn quá fallback về default mask
    [InlineData(null, "***@***.***")]
    public async Task MaskEmail_ShouldMaskCorrectly(string? email, string expectedMask)
    {
        // Vì hàm MaskEmail là private, ta test gián tiếp qua Handle
        // Hoặc có thể dùng Reflection nếu muốn test riêng, nhưng test qua Handle là đủ integration

        // --- ARRANGE ---
        var command = new LoginCommand("user", "pass");
        var user = new User { UserName = "user", Email = email, PasswordHash = "hash" };

        MockUserRepo.Setup(x => x.GetByUserNameAsync("user", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        MockHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        MockOtpService.Setup(x => x.GenerateOtp(It.IsAny<int>())).Returns(("sid", "code"));

        // --- ACT ---
        // Chúng ta phải wait task vì method là async
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.MaskedEmail.Should().Be(expectedMask);
    }
}