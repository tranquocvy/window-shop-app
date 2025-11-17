using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Users;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpUserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/users";

        public HttpUserService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<ResponseWrapper<List<UserDto>>> GetAllUsersAsync()
        {
            return _httpClient.GetWrapperFromJsonAsync<List<UserDto>>(BaseUrl, "Failed to retrieve users");
        }

        public Task<ResponseWrapper<UserDto>> GetUserByIdAsync(int id)
        {
            return _httpClient.GetWrapperFromJsonAsync<UserDto>($"{BaseUrl}/{id}", "Failed to retrieve user");
        }

        public async Task<ResponseWrapper<UserDto>> CreateUserAsync(UserCreateUpdateDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
            return await response.EnsureSuccessAndReadWrapperAsync<UserDto>("Failed to create user");
        }

        public async Task<ResponseWrapper<UserDto>> UpdateUserAsync(int id, UserCreateUpdateDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
            return await response.EnsureSuccessAndReadWrapperAsync<UserDto>("Failed to update user");
        }

        public async Task<ResponseWrapper<bool>> DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            return await response.EnsureSuccessAndReadWrapperAsync<bool>("Failed to delete user");
        }
    }
}
