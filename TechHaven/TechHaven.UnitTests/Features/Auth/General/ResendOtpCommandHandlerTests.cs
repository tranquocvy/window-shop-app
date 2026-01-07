using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.ResendOtp;
using TechHaven.Domain.Entities;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class ResendOtpCommandHandlerTests : UnitTestBase
{
    private readonly ResendOtpCommandHandler _handler;

    public ResendOtpCommandHandlerTests()
    {
        _handler = new ResendOtpCommandHandler(
            MockUow.Object,
            MockOtpService.Object,     // Từ UnitTestBase
            MockEmailService.Object    // Từ UnitTestBase
        );
    }

    [Fact]
    public async Task Handle_ValidSession_ShouldInvalidateOld_GenerateNew_AndSendEmail()
    {
        // --- ARRANGE ---
        var oldSessionId = Guid.NewGuid().ToString();
        var newSessionId = Guid.NewGuid().ToString();
        var newOtpCode = "654321";

        var command = new ResendOtpCommand(oldSessionId);

        var user = new User
        {
            UserId = 1,
            Email = "user@test.com",
            UserFullName = "Test User"
        };

        // 1. Mock Check Session cũ -> Trả về UserId (Hợp lệ)
        MockOtpService.Setup(x => x.GetUserIdFromSession(oldSessionId))
            .Returns(user.UserId);

        // 2. Mock Get User -> Found
        MockUserRepo.Setup(x => x.GetByIdAsync(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // 3. Mock Generate OTP Mới
        MockOtpService.Setup(x => x.GenerateOtp(user.UserId))
            .Returns((newSessionId, newOtpCode));

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.IsOtpResent.Should().BeTrue();
        result.NewOtpSessionId.Should().Be(newSessionId);

        // Verify Critical Flow:
        // 1. Phải hủy Session cũ
        MockOtpService.Verify(x => x.InvalidateOtp(oldSessionId), Times.Once);

        // 2. Phải tạo OTP mới
        MockOtpService.Verify(x => x.GenerateOtp(user.UserId), Times.Once);

        // 3. Phải gửi email mới
        MockEmailService.Verify(x => x.SendOtpEmailAsync(
            user.Email,
            user.UserFullName,
            newOtpCode,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptySessionId_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new ResendOtpCommand(""); // Empty

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        // Dùng ContainKey vì ValidationException lưu dạng Dictionary
        ex.Errors.Should().ContainKey("OtpSessionId");
    }

    [Fact]
    public async Task Handle_ExpiredOrInvalidSession_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new ResendOtpCommand("invalid_session");

        // Mock Check Session -> Trả về null (Không tìm thấy hoặc hết hạn)
        MockOtpService.Setup(x => x.GetUserIdFromSession("invalid_session"))
            .Returns((int?)null);

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        ex.Errors.Should().ContainKey("OtpSessionId");

        // Verify: Không được gọi Generate hay Send Mail
        MockOtpService.Verify(x => x.GenerateOtp(It.IsAny<int>()), Times.Never);
        MockEmailService.Verify(x => x.SendOtpEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // --- ARRANGE ---
        var sessionId = "session_ok";
        var command = new ResendOtpCommand(sessionId);

        // Session hợp lệ trả về UserId = 99
        MockOtpService.Setup(x => x.GetUserIdFromSession(sessionId)).Returns(99);

        // Nhưng User 99 không tồn tại trong DB (Data inconsistency)
        MockUserRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // --- ACT & ASSERT ---
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}