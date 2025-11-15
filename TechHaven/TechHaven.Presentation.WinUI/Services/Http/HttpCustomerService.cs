using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Presentation.WinUI.Services.Interfaces;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpCustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/customers";

        public HttpCustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ResponseWrapper<List<CustomerDto>>> GetAllCustomersAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<List<CustomerDto>>>(BaseUrl);
                return response ?? new ResponseWrapper<List<CustomerDto>> 
                { 
                  Success = false, 
                  Message = "No response", 
                };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<List<CustomerDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve customers",
                    Errors = new List<string> { ex.Message },
                };
            }
        }

        public async Task<ResponseWrapper<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ResponseWrapper<CustomerDto>>($"{BaseUrl}/{id}");
                return response ?? new ResponseWrapper<CustomerDto>
                {
                    Success = false,
                    Message = "No response",
                };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<CustomerDto> 
                { 
                    Success = false, 
                    Message = "Failed to retrieve customer", 
                    Errors = new List<string> { ex.Message } 
                };
            }
        }

        public async Task<ResponseWrapper<CustomerDto>> CreateCustomerAsync(CustomerCreateUpdateDto customerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, customerDto);
                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseWrapper<CustomerDto> 
                    { 
                        Success = false, 
                        Message = $"API returned {(int)response.StatusCode}" 
                    };
                }

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<CustomerDto>>();
                return wrapper ?? new ResponseWrapper<CustomerDto> 
                {
                    Success = false, 
                    Message = "No response" 
                };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<CustomerDto> 
                { 
                    Success = false, 
                    Message = "Failed to create customer", 
                    Errors = new List<string> { ex.Message } 
                };
            }
        }

        public async Task<ResponseWrapper<CustomerDto>> UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<CustomerDto> 
                    { 
                        Success = false, 
                        Message = $"API returned {(int)response.StatusCode}" 
                    };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<CustomerDto>>();
                if (wrapper != null)
                {
                    return new ResponseWrapper<CustomerDto>
                    {
                        Success = wrapper.Success,
                        Message = wrapper.Message,
                        Errors = wrapper.Errors,
                        Data = wrapper.Data
                    };
                }

                return new ResponseWrapper<CustomerDto> 
                { 
                    Success = false, 
                    Message = "No response" 
                };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<CustomerDto> 
                { 
                    Success = false, 
                    Message = "Failed to update customer", 
                    Errors = new List<string> { ex.Message } 
                };
            }
        }

        public async Task<ResponseWrapper<bool>> DeleteCustomerAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<bool> 
                    { 
                        Success = false, 
                        Message = $"API returned {(int)response.StatusCode}" 
                    };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<bool>>();
                return wrapper ?? new ResponseWrapper<bool> 
                { 
                    Success = false, 
                    Message = "No response" 
                };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<bool> 
                { 
                    Success = false, 
                    Message = "Failed to delete customer", 
                    Errors = new List<string> { ex.Message } 
                };
            }
        }

        public async Task<ResponseWrapper<List<CustomerDto>>> QueryCustomersAsync(CustomerQueryDto query)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/query", query);
                if (!response.IsSuccessStatusCode)
                    return new ResponseWrapper<List<CustomerDto>> 
                    { 
                        Success = false, 
                        Message = $"API returned {(int)response.StatusCode}" 
                    };

                var wrapper = await response.Content.ReadFromJsonAsync<ResponseWrapper<List<CustomerDto>>>();
                return wrapper ?? new ResponseWrapper<List<CustomerDto>> 
                { 
                    Success = false, 
                    Message = "No response" 
                };
            }
            catch (Exception ex)
            {
                return new ResponseWrapper<List<CustomerDto>> 
                { 
                    Success = false, 
                    Message = "Failed to query customers", 
                    Errors = new List<string> { ex.Message } 
                };
            }
        }
    }
}
