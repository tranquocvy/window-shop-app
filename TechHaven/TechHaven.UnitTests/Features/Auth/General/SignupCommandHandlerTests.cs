using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Application.Features.Auth.Signup;
using TechHaven.Domain.Entities;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class SignupCommandHandlerTests : UnitTestBase
{
    private readonly SignupCommandHandler _handler;
    private readonly Mock<ILogger<SignupCommandHandler>> _mockLogger;

    public SignupCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<SignupCommandHandler>>();

        _handler = new SignupCommandHandler(
            MockUow.Object,
            _mockLogger.Object,
            MockHasher.Object // Từ UnitTestBase
        );
    }

    [Fact]
    public async Task Handle_ValidNewUser_ShouldHashPassword_And_CreateUser()
    {
        // --- ARRANGE ---
        var command = new SignupCommand(
            "Test User",
            "test@email.com",
            "testuser",
            "password123",
            2 // RoleId
        );

        // 1. Mock Check trùng Username: Không tìm thấy (trả về null)
        MockUserRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // 2. Mock Hasher
        MockHasher.Setup(x => x.HashPassword("password123")).Returns("hashed_secret");

        // 3. Mock AddAsync để giả lập việc gán ID khi lưu
        MockUserRepo.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, ct) => u.UserId = 10);

        // --- ACT ---
        var result = await _handler.Handle(command, CancellationToken.None);

        // --- ASSERT ---
        result.Should().NotBeNull();
        result.UserId.Should().Be(10);
        result.UserName.Should().Be("testuser");
        result.Message.Should().Contain("successfully");

        // Verify Critical Logic:
        // 1. Password phải được hash
        MockHasher.Verify(x => x.HashPassword("password123"), Times.Once);

        // 2. Phải gọi Add và SaveChanges
        MockUserRepo.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.UserName == "testuser" &&
            u.PasswordHash == "hashed_secret" &&
            u.IsActive == true // Default value check
        ), It.IsAny<CancellationToken>()), Times.Once);

        MockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateUserName_ShouldThrowValidationException()
    {
        // --- ARRANGE (CHUẨN BỊ) ---
        // Tạo một command giả với tên "existing_user"
        var command = new SignupCommand("Test", "t@t.com", "existing_user", "pass", 1);

        // Tạo một user giả để giả vờ là user này đang nằm trong Database
        var existingUser = new User { UserName = "existing_user" };

        // Dòng này nói với MockRepo: "Hễ ai gọi hàm FirstOrDefaultAsync tìm user nào đó, 
        // thì hãy trả về cái 'existingUser' kia nhé (đừng trả về null)".
        MockUserRepo.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // --- ACT & ASSERT (HÀNH ĐỘNG & KIỂM TRA) ---

        // DÒNG QUAN TRỌNG NHẤT:
        // Assert.ThrowsAsync<ValidationException>: Đây là hàm "bẫy lỗi".
        // Nó bảo: "Tôi mong chờ đoạn code bên trong () sẽ BỊ CRASH với lỗi ValidationException".
        // - Nếu code chạy mượt (không lỗi) -> Test Fail (Sai mong đợi).
        // - Nếu code lỗi khác (VD: NullReference) -> Test Fail.
        // - Nếu code lỗi đúng ValidationException -> Test Pass và GÁN cái lỗi đó vào biến 'ex'.
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(command, CancellationToken.None));

        // --- KIỂM TRA CHI TIẾT CÁI LỖI (biến ex) ---

        // ex.Errors: Là cái Dictionary chứa danh sách lỗi mà Handler ném ra.
        // Kiểm tra xem trong đống lỗi đó có key nào tên là "UserName" không?
        // (Để đảm bảo lỗi hiển thị đúng chỗ ô input Username trên UI).
        ex.Errors.Should().ContainKey("UserName");

        // ex.Errors["UserName"]: Lấy ra danh sách các câu thông báo lỗi của key "UserName".
        // .Should().Contain(...): Kiểm tra xem câu thông báo đó có chứa đúng nội dung ta muốn không.
        ex.Errors["UserName"].Should().Contain($"Username '{command.UserName}' is already taken");

        // --- VERIFY (KIỂM TRA HẬU QUẢ) ---

        // Đảm bảo rằng vì đã lỗi trùng tên, nên hệ thống KHÔNG ĐƯỢC PHÉP băm mật khẩu.
        MockHasher.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);

        // Đảm bảo rằng KHÔNG ĐƯỢC PHÉP lưu user này vào Database.
        MockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}