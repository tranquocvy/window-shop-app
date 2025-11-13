using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Users;
namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockAuthService
    {
        private readonly MockUserService _userService = new();

        public async Task<UserDto?> VerifyLoginAsync(string username, string password)
        {
            var users = await _userService.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.UserName == username);

            if (user == null || !user.IsActive || password != username)
                return null;

            return user;
        }
    }
}
