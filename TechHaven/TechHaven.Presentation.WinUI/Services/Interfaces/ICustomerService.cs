using System.Collections.Generic;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ResponseWrapper<List<CustomerDto>>> GetAllCustomersAsync();
        Task<ResponseWrapper<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<ResponseWrapper<CustomerDto>> CreateCustomerAsync(CustomerUpsertRequestDto Customerdto);
        Task<ResponseWrapper<CustomerDto>> UpdateCustomerAsync(int id, CustomerUpsertRequestDto dto);
        Task<ResponseWrapper<bool>> DeleteCustomerAsync(int id);
        Task<ResponseWrapper<List<CustomerDto>>> QueryCustomersAsync(CustomerListQueryDto query);
    }
}
