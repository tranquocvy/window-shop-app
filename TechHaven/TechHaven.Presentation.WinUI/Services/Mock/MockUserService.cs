using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockUserService : IUserService
    {
        private readonly List<UserDto> _mockUsers;
        private bool _hasSeenGuide;

        public MockUserService()
        {
            _mockUsers = new List<UserDto>
            {
                new UserDto
                {
                    UserId = 1,
                    UserFullName = "Nguyễn Khắc Vượng",
                    UserName = "admin",
                    RoleId = 1,
                    RoleName = "Admin",
                    IsActive = true
                },
                new UserDto
                {
                    UserId = 2,
                    UserFullName = "Nguyễn Phúc Hậu",
                    UserName = "seller",
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
        public Task<ResponseWrapper<List<UserDto>>> GetAllUsersAsync()
        {
            var response = new ResponseWrapper<List<UserDto>>
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = _mockUsers
            };
            return Task.FromResult(response);
        }

        // Lấy người dùng theo ID
        public Task<ResponseWrapper<UserDto>> GetUserByIdAsync(int id)
        {
            var user = _mockUsers.FirstOrDefault(u => u.UserId == id);
            var response = new ResponseWrapper<UserDto>
            {
                Success = user != null,
                Message = user != null ? "User retrieved successfully" : "User not found",
                Data = user
            };
            return Task.FromResult(response);
        }

        // Tạo người dùng mới
        public Task<ResponseWrapper<UserDto>> CreateUserAsync(UserCreateUpdateDto dto)
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

            var response = new ResponseWrapper<UserDto>
            {
                Success = true,
                Message = "User created successfully",
                Data = newUser
            };
            return Task.FromResult(response);
        }

        // Cập nhật thông tin người dùng
        public Task<ResponseWrapper<UserDto>> UpdateUserAsync(int id, UserCreateUpdateDto dto)
        {
            var existing = _mockUsers.FirstOrDefault(u => u.UserId == id);
            if (existing != null)
            {
                existing.UserFullName = dto.UserFullName;
                existing.UserName = dto.UserName;
                existing.RoleId = dto.RoleId;
                existing.RoleName = GetRoleName(dto.RoleId);
                existing.IsActive = dto.IsActive;
            }

            var response = new ResponseWrapper<UserDto>
            {
                Success = existing != null,
                Message = existing != null ? "User updated successfully" : "User not found",
                Data = existing
            };
            return Task.FromResult(response);
        }

        // Xóa người dùng
        public Task<ResponseWrapper<bool>> DeleteUserAsync(int id)
        {
            var existing = _mockUsers.FirstOrDefault(u => u.UserId == id);
            bool success = false;

            if (existing != null)
            {
                _mockUsers.Remove(existing);
                success = true;
            }

            var response = new ResponseWrapper<bool>
            {
                Success = success,
                Message = success ? "User deleted successfully" : "User not found",
                Data = success
            };
            return Task.FromResult(response);
        }

        public Task<ResponseWrapper<bool>> UpdateGuideStatusAsync(bool hasSeenGuide)
        {
            _hasSeenGuide = hasSeenGuide;
            var response = new ResponseWrapper<bool>
            {
                Success = true,
                Message = "Guide status updated",
                Data = _hasSeenGuide
            };
            return Task.FromResult(response);
        }

        // Helper: Lấy tên role theo ID
        private static string GetRoleName(int roleId)
        {
            return roleId switch
            {
                1 => "Admin",
                2 => "Seller",
                _ => "Other"
            };
        }
    }
}
