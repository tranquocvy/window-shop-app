using System.Collections.Generic;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseWrapper<List<UserDto>>> GetAllUsersAsync();
        Task<ResponseWrapper<UserDto>> GetUserByIdAsync(int id);
        Task<ResponseWrapper<UserDto>> CreateUserAsync(UserCreateUpdateDto dto);
        Task<ResponseWrapper<UserDto>> UpdateUserAsync(int id, UserCreateUpdateDto dto);
        Task<ResponseWrapper<bool>> DeleteUserAsync(int id);
        Task<ResponseWrapper<bool>> UpdateGuideStatusAsync(bool hasSeenGuide);
    }
}
