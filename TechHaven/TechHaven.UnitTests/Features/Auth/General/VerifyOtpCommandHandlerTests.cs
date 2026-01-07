using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.VerifyOtp;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class VerifyOtpCommandHandlerTests : UnitTestBase
{
    private readonly VerifyOtpCommandHandler _handler;

    public VerifyOtpCommandHandlerTests()
    { 

        _handler = new VerifyOtpCommandHandler(
            MockUow.Object,
            MockOtpService.Object, // Từ UnitTestBase
            MockJwtTokenService.Object
        );
    }

    [Fact]
    public async Task Handle_ValidOtp_ShouldGenerateTokens_And_UpdateUser()
    {
        // --- ARRANGE ---
        var sessionId = Guid.NewGuid().ToString();
        var otpCode = "123456";
        var command = new VerifyOtpCommand(sessionId, otpCode);

        var user = new User
        {
            UserId = 1,
            UserName = "testuser",
            UserFullName = "Test User",
            RoleId = 1,
            Role = new Role { RoleId = 1, RoleName = "Admin" }
        };

        // 1. Mock Validate OTP -> Trả về UserId (Hợp lệ)
        MockOtpService.Setup(x => x.ValidateOtp(sessionId, otpCode)).Returns(1);

        // 2. Mock User Found (Có sẵn Role)
        MockUserRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // 3. Mock Generate Tokens
        MockJwtTokenService.Setup(x => x.GenerateAccessToken(user, null)).Returns("access_token_123");
        MockJwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token_abc");

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token_123");
        result.RefreshToken.Should().Be("refresh_token_abc");
        result.UserName.Should().Be("testuser");

        // Verify: User được update refresh token
        user.RefreshToken.Should().Be("refresh_token_abc");
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        // Verify: Các hàm quan trọng được gọi
        MockOtpService.Verify(x => x.InvalidateOtp(sessionId), Times.Once); // Hủy OTP sau khi dùng
        MockUserRepo.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidOtp_ShouldThrowValidationException()
    {
        // --- ARRANGE ---
        var command = new VerifyOtpCommand("session_id", "wrong_code");

        // Mock Validate OTP -> Trả về null (Không hợp lệ)
        MockOtpService.Setup(x => x.ValidateOtp(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((int?)null);

        // --- ACT & ASSERT ---
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        // Dùng ContainKey cho Dictionary Errors
        ex.Errors.Should().ContainKey("OtpCode");

        // Verify: Không được gọi DB hay tạo Token
        MockUserRepo.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // --- ARRANGE ---
        var command = new VerifyOtpCommand("session_id", "123456");

        // OTP Valid nhưng User ID không tồn tại trong DB (Data integrity issue)
        MockOtpService.Setup(x => x.ValidateOtp(It.IsAny<string>(), It.IsAny<string>())).Returns(99);

        MockUserRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // --- ACT & ASSERT ---
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserMissingRole_ShouldFetchWithRole()
    {
        // --- ARRANGE ---
        var command = new VerifyOtpCommand("session", "code");

        // User ban đầu không có Role (Role = null)
        var userWithoutRole = new User { UserId = 1, RoleId = 1, Role = null };
        // User đầy đủ lấy từ hàm GetWithRoleAsync
        var userWithRole = new User { UserId = 1, RoleId = 1, Role = new Role { RoleName = "User" } };

        MockOtpService.Setup(x => x.ValidateOtp(It.IsAny<string>(), It.IsAny<string>())).Returns(1);

        // Lần đầu get trả về user thiếu Role
        MockUserRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userWithoutRole);

        // Mock hàm GetWithRoleAsync trả về list chứa user đầy đủ
        MockUserRepo.Setup(x => x.GetWithRoleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { userWithRole });

        MockJwtTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), null)).Returns("token");
        MockJwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.RoleName.Should().Be("User");

        // Verify: Đã gọi hàm GetWithRoleAsync để "chữa cháy" việc thiếu Role
        MockUserRepo.Verify(x => x.GetWithRoleAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}