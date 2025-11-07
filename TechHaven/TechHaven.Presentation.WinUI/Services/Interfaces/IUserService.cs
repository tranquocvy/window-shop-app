using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(UserCreateUpdateDto dto);
        Task<UserDto?> UpdateUserAsync(int id, UserCreateUpdateDto dto);
        Task<bool> DeleteUserAsync(int id);
        Task<List<UserDto>> QueryUsersAsync(UserQueryDto query);
    }
}
