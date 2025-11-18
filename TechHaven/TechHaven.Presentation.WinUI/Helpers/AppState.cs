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
        private const string DefaultApiBaseUrl = "http://localhost:5207/";

        public static UserDto? CurrentUser { get; set; }
        public static bool IsLoggedIn => CurrentUser != null;

        public static Uri ApiBaseUri { get; } = new(
            Environment.GetEnvironmentVariable("TECHHAVEN_API_BASEURL") ??
            DefaultApiBaseUrl);
    }
}
