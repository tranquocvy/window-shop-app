using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CustomerCreateUpdateDto Customerdto);
        Task<CustomerDto?> UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<List<CustomerDto>> QueryCustomersAsync(CustomerQueryDto query);

    }
}
