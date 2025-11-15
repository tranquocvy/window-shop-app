using Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Customers;
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

        public async Task<ResponseWrapper<List<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<List<UserDto>>>(BaseUrl);
                return response ?? new ResponseWrapper<List<UserDto>> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<List<UserDto>> { Success = false, Message = "Failed to retrieve users", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<UserDto>> GetUserByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<UserDto>>($"{BaseUrl}/{id}");
                return response ?? new ResponseWrapper<UserDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<UserDto> { Success = false, Message = "Failed to retrieve user", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<UserDto>> CreateUserAsync(UserCreateUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<UserDto> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<UserDto>>();
                return wrapper ?? new ResponseWrapper<UserDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<UserDto> { Success = false, Message = "Failed to create user", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<UserDto>> UpdateUserAsync(int id, UserCreateUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<UserDto> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<UserDto>>();
                return wrapper ?? new ResponseWrapper<UserDto> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<UserDto> { Success = false, Message = "Failed to update user", Errors = new List<string> { ex.Message } };
            }
        }

        public async Task<ResponseWrapper<bool>> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<bool> { Success = false, Message = $"API returned {(int)response.StatusCode}" };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<bool>>();
                return wrapper ?? new ResponseWrapper<bool> { Success = false, Message = "No response" };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<bool> { Success = false, Message = "Failed to delete user", Errors = new List<string> { ex.Message } };
            }
        }
    }
}
