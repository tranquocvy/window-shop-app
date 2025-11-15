using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ResponseWrapper<List<CustomerDto>>> GetAllCustomersAsync();
        Task<ResponseWrapper<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<ResponseWrapper<CustomerDto>> CreateCustomerAsync(CustomerCreateUpdateDto Customerdto);
        Task<ResponseWrapper<CustomerDto>> UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto);
        Task<ResponseWrapper<bool>> DeleteCustomerAsync(int id);
        Task<ResponseWrapper<List<CustomerDto>>> QueryCustomersAsync(CustomerQueryDto query);
    }
}
