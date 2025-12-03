using System.Collections.Generic;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface ICustomerService
    {
        /// <summary>
        /// Query customers with pagination, filtering, and sorting
        /// GET api/Customer/
        /// </summary>
        Task<ResponseWrapper<PagingResponse<CustomerDto>>> QueryCustomersAsync(CustomerListQueryDto query);

        /// <summary>
        /// Get customer by ID
        /// GET api/Customer/{id}
        /// </summary>
        Task<ResponseWrapper<CustomerDto>> GetCustomerByIdAsync(int id);
        /// <summary>
        /// Create new customer
        /// POST api/Customer/
        /// </summary>
        Task<ResponseWrapper<CustomerDto>> CreateCustomerAsync(CustomerUpsertRequestDto dto);

        /// <summary>
        /// Update existing customer
        /// PUT api/Customer/{id}
        /// </summary>
        Task<ResponseWrapper<CustomerDto>> UpdateCustomerAsync(int id, CustomerUpsertRequestDto dto);

        /// <summary>
        /// Delete customer
        /// DELETE api/Customer/{id}
        /// </summary>
        Task<ResponseWrapper<bool>> DeleteCustomerAsync(int id);
    }
}
