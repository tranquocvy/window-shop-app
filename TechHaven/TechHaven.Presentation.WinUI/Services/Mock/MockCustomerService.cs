using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    internal class MockCustomerService : ICustomerService
    {
        private readonly List<CustomerDto> _mockCustomers;

        public MockCustomerService()
        {
            _mockCustomers = new List<CustomerDto>
            {
                new CustomerDto
                {
                    CustomerId = 1,
                    CustomerName = "Nguyễn Văn A",
                    PhoneNumber = "0901234567",
                    Email = "a.nguyen@example.com",
                    Address = "Hà Nội",
                    Type = CustomerType.Regular,
                    TotalPurchased = 12_000_000,
                    Note = "Khách hàng thường xuyên"
                },
                new CustomerDto
                {
                    CustomerId = 2,
                    CustomerName = "Trần Thị B",
                    PhoneNumber = "0912345678",
                    Email = "b.tran@example.com",
                    Address = "TP.HCM",
                    Type = CustomerType.Student,
                    TotalPurchased = 3_500_000,
                    Note = "Sinh viên, được giảm 10%"
                },
                new CustomerDto
                {
                    CustomerId = 3,
                    CustomerName = "Phạm Minh C",
                    PhoneNumber = "0988888888",
                    Email = "c.pham@example.com",
                    Address = "Đà Nẵng",
                    Type = CustomerType.VIP,
                    TotalPurchased = 75_000_000,
                    Note = "Khách VIP, ưu tiên hỗ trợ"
                }
            };
        }

        // Lấy tất cả khách hàng
        public Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            return Task.FromResult(_mockCustomers);
        }

        // Lấy khách hàng theo ID
        public Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = _mockCustomers.FirstOrDefault(c => c.CustomerId == id);
            return Task.FromResult(customer);
        }

        // Tạo khách hàng mới
        public Task<CustomerDto> CreateCustomerAsync(CustomerCreateUpdateDto dto)
        {
            var newCustomer = new CustomerDto
            {
                CustomerId = _mockCustomers.Max(c => c.CustomerId) + 1,
                CustomerName = dto.CustomerName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                Type = dto.Type,
                TotalPurchased = 0,
                Note = dto.Note
            };
            _mockCustomers.Add(newCustomer);
            return Task.FromResult(newCustomer);
        }

        // Cập nhật thông tin khách hàng
        public Task<CustomerDto?> UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto)
        {
            var existing = _mockCustomers.FirstOrDefault(c => c.CustomerId == id);
            if (existing == null)
                return Task.FromResult<CustomerDto?>(null);

            existing.CustomerName = dto.CustomerName;
            existing.PhoneNumber = dto.PhoneNumber;
            existing.Email = dto.Email;
            existing.Address = dto.Address;
            existing.Type = dto.Type;
            existing.Note = dto.Note;

            return Task.FromResult<CustomerDto?>(existing);
        }

        // Xoá khách hàng
        public Task<bool> DeleteCustomerAsync(int id)
        {
            var existing = _mockCustomers.FirstOrDefault(c => c.CustomerId == id);
            if (existing == null) return Task.FromResult(false);
            _mockCustomers.Remove(existing);
            return Task.FromResult(true);
        }

        // Lọc & sắp xếp (CustomerQueryDto)
        public Task<List<CustomerDto>> QueryCustomersAsync(CustomerQueryDto query)
        {
            IEnumerable<CustomerDto> result = _mockCustomers;

            // Lọc theo từ khóa (tên, sđt, email)
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                result = result.Where(c =>
                    c.CustomerName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.PhoneNumber.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email?.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Lọc theo loại khách hàng
            if (query.Type.HasValue)
                result = result.Where(c => c.Type == query.Type.Value);

            // Sắp xếp
            if (query.Sorting != null)
            {
                if (query.Sorting.SortBy?.Equals("name", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = query.Sorting.Desc
                        ? result.OrderByDescending(c => c.CustomerName)
                        : result.OrderBy(c => c.CustomerName);
                }
                else if (query.Sorting.SortBy?.Equals("total", StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = query.Sorting.Desc
                        ? result.OrderByDescending(c => c.TotalPurchased)
                        : result.OrderBy(c => c.TotalPurchased);
                }
            }

            // Phân trang
            if (query.PageNumber > 0 && query.PageSize > 0)
            {
                result = result
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);
            }

            return Task.FromResult(result.ToList());
        }
    }
}
