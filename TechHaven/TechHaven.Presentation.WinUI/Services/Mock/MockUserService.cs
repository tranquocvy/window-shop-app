using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockUserService
    {
        private readonly List<UserDto> _mockUsers;

        public MockUserService()
        {
            _mockUsers = new List<UserDto>
            {
                new UserDto
                {
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    UserName = "khacvuong",
                    RoleId = 1,
                    RoleName = "Admin",
                    IsActive = true
                },
                new UserDto
                {
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    UserName = "phuchau",
                    RoleId = 2,
                    RoleName = "Seller",
                    IsActive = true
                },
                new UserDto
                {
                    UserId = 3,
                    UserFullName = "Trần Quốc Vỹ",
                    UserName = "quocvy",
                    RoleId = 2,
                    RoleName = "Seller",
                    IsActive = false
                }
            };
        }

        // Lấy toàn bộ người dùng
        public Task<List<UserDto>> GetAllUsersAsync()
        {
            return Task.FromResult(_mockUsers);
        }

        // Lấy người dùng theo ID
        public Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = _mockUsers.FirstOrDefault(u => u.UserId == id);
            return Task.FromResult(user);
        }

        // Tạo người dùng mới
        public Task<UserDto> CreateUserAsync(UserCreateUpdateDto dto)
        {
            var newUser = new UserDto
            {
                UserId = _mockUsers.Max(u => u.UserId) + 1,
                UserFullName = dto.UserFullName,
                UserName = dto.UserName,
                RoleId = dto.RoleId,
                RoleName = GetRoleName(dto.RoleId),
                IsActive = dto.IsActive
            };
            _mockUsers.Add(newUser);
            return Task.FromResult(newUser);
        }

        // Cập nhật thông tin người dùng
        public Task<UserDto?> UpdateUserAsync(int id, UserCreateUpdateDto dto)
        {
            var existing = _mockUsers.FirstOrDefault(u => u.UserId == id);
            if (existing == null)
                return Task.FromResult<UserDto?>(null);

            existing.UserFullName = dto.UserFullName;
            existing.UserName = dto.UserName;
            existing.RoleId = dto.RoleId;
            existing.RoleName = GetRoleName(dto.RoleId);
            existing.IsActive = dto.IsActive;

            return Task.FromResult<UserDto?>(existing);
        }

        // Xóa người dùng
        public Task<bool> DeleteUserAsync(int id)
        {
            var existing = _mockUsers.FirstOrDefault(u => u.UserId == id);
            if (existing == null) return Task.FromResult(false);
            _mockUsers.Remove(existing);
            return Task.FromResult(true);
        }

        // Helper: Lấy tên role theo ID
        private static string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Admin",
                2 => "Seller",
                _ => "Khác"
            };
        }
    }
}
