using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class AppState
    {
        public static UserDto? CurrentUser { get; set; }
        public static bool IsLoggedIn => CurrentUser != null;
    }
}
