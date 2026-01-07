using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.RefreshToken;
using TechHaven.Domain.Entities;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class RefreshTokenCommandHandlerTests : UnitTestBase
{
	private readonly RefreshTokenCommandHandler _handler;

	public RefreshTokenCommandHandlerTests()
	{
		// MockJwtTokenService lấy từ UnitTestBase
		_handler = new RefreshTokenCommandHandler(MockUow.Object, MockJwtTokenService.Object);
	}

	[Fact]
	public async Task Handle_ValidToken_ShouldRotateTokens_And_SaveChanges()
	{
		// --- ARRANGE ---
		var oldRefreshToken = "valid_refresh_token";
		var command = new RefreshTokenCommand(oldRefreshToken);

		var user = new User
		{
			UserId = 1,
			UserName = "user",
			RefreshToken = oldRefreshToken,
			RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1), // Chưa hết hạn
			RoleId = 1,
			Role = new Role { RoleName = "Admin" }
		};

		// 1. Mock GetAll (Lưu ý: Code hiện tại load all users lên memory)
		MockUserRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<User> { user });

		// 2. Mock Generate New Tokens
		MockJwtTokenService.Setup(x => x.GenerateAccessToken(user, It.IsAny<string>())).Returns("new_access");
		MockJwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns("new_refresh");

		// --- ACT ---
		var result = await _handler.Handle(command, CancellationToken.None);

		// --- ASSERT ---
		result.Should().NotBeNull();
		result.AccessToken.Should().Be("new_access");
		result.NewRefreshToken.Should().Be("new_refresh");

		// Verify: User được cập nhật token mới và hạn mới
		user.RefreshToken.Should().Be("new_refresh");
		user.RefreshTokenExpiryTime.Should().BeAfter(DateTime.UtcNow.AddDays(6));

		// Verify: SaveChanges được gọi
		MockUserRepo.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
		MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_InvalidToken_ShouldThrowValidationException()
	{
		// --- ARRANGE ---
		var command = new RefreshTokenCommand("invalid_token");

		// Mock GetAll trả về list user nhưng KHÔNG có user nào khớp token
		MockUserRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<User>
			{
				new User { RefreshToken = "other_token" }
			});

		// --- ACT & ASSERT ---
		var ex = await Assert.ThrowsAsync<ValidationException>(
			() => _handler.Handle(command, CancellationToken.None));

		ex.Errors.Should().ContainKey("RefreshToken");
		ex.Errors["RefreshToken"].Should().Contain("Invalid refresh token");
	}

	[Fact]
	public async Task Handle_ExpiredToken_ShouldThrowValidationException()
	{
		// --- ARRANGE ---
		var expiredToken = "expired_token";
		var command = new RefreshTokenCommand(expiredToken);

		var user = new User
		{
			RefreshToken = expiredToken,
			RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // Đã hết hạn hôm qua
		};

		MockUserRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<User> { user });

		// --- ACT & ASSERT ---
		var ex = await Assert.ThrowsAsync<ValidationException>(
			() => _handler.Handle(command, CancellationToken.None));

		ex.Errors["RefreshToken"].Should().Contain("Refresh token has expired");
	}

	[Fact]
	public async Task Handle_UserMissingRole_ShouldFetchRole()
	{
		// --- ARRANGE ---
		var token = "valid_token";
		var command = new RefreshTokenCommand(token);

		// User ban đầu thiếu Role
		var userNoRole = new User { UserId = 1, RefreshToken = token, RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1), Role = null };
		// User đầy đủ
		var userWithRole = new User { UserId = 1, RefreshToken = token, Role = new Role { RoleName = "User" } };

		MockUserRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<User> { userNoRole });

		// Mock GetWithRoleAsync (Logic fallback)
		MockUserRepo.Setup(x => x.GetWithRoleAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<User> { userWithRole });

		MockJwtTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<string>())).Returns("token");
		MockJwtTokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

		// --- ACT ---
		await _handler.Handle(command, CancellationToken.None);

		// --- ASSERT ---
		// Verify: Đã gọi hàm GetWithRoleAsync
		MockUserRepo.Verify(x => x.GetWithRoleAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}