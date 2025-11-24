using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    internal class MockCustomerService : ICustomerService
    {
        private readonly List<CustomerDto> _mockCustomers;

        public MockCustomerService()
        {
            _mockCustomers = new List<CustomerDto>{
                new CustomerDto { CustomerId = 1, CustomerName = "Nguyễn Văn A", PhoneNumber = "0901234567", Email = "a.nguyen@example.com", Address = "Hà Nội", Type = CustomerType.Regular, TotalPurchased = 12_000_000, Note = "Khách hàng thường xuyên" },
                new CustomerDto { CustomerId = 2, CustomerName = "Trần Thị B", PhoneNumber = "0912345678", Email = "b.tran@example.com", Address = "TP.HCM", Type = CustomerType.Student, TotalPurchased = 3_500_000, Note = "Sinh viên, được giảm 10%" },
                new CustomerDto { CustomerId = 3, CustomerName = "Phạm Minh C", PhoneNumber = "0988888888", Email = "c.pham@example.com", Address = "Đà Nẵng", Type = CustomerType.VIP, TotalPurchased = 75_000_000, Note = "Khách VIP, ưu tiên hỗ trợ" },
                new CustomerDto { CustomerId = 4, CustomerName = "Lê Thị D", PhoneNumber = "0901112233", Email = "d.le@example.com", Address = "Hải Phòng", Type = CustomerType.Regular, TotalPurchased = 8_200_000, Note = "Khách mới" },
                new CustomerDto { CustomerId = 5, CustomerName = "Vũ Văn E", PhoneNumber = "0911223344", Email = "e.vu@example.com", Address = "Cần Thơ", Type = CustomerType.Student, TotalPurchased = 1_500_000, Note = "Sinh viên, hay mua online" },
                new CustomerDto { CustomerId = 6, CustomerName = "Ngô Thị F", PhoneNumber = "0922334455", Email = "f.ngo@example.com", Address = "Hà Nội", Type = CustomerType.VIP, TotalPurchased = 60_000_000, Note = "Khách VIP" },
                new CustomerDto { CustomerId = 7, CustomerName = "Đặng Văn G", PhoneNumber = "0933445566", Email = "g.dang@example.com", Address = "TP.HCM", Type = CustomerType.Regular, TotalPurchased = 15_000_000, Note = "" },
                new CustomerDto { CustomerId = 8, CustomerName = "Phan Thị H", PhoneNumber = "0944556677", Email = "h.phan@example.com", Address = "Đà Nẵng", Type = CustomerType.Student, TotalPurchased = 2_800_000, Note = "Mua theo nhóm" },
                new CustomerDto { CustomerId = 9, CustomerName = "Trương Minh I", PhoneNumber = "0955667788", Email = "i.truong@example.com", Address = "Hải Phòng", Type = CustomerType.VIP, TotalPurchased = 90_000_000, Note = "Khách VIP" },
                new CustomerDto { CustomerId = 10, CustomerName = "Bùi Thị J", PhoneNumber = "0966778899", Email = "j.bui@example.com", Address = "Cần Thơ", Type = CustomerType.Regular, TotalPurchased = 5_000_000, Note = "" },
                new CustomerDto { CustomerId = 11, CustomerName = "Nguyễn Văn K", PhoneNumber = "0977889900", Email = "k.nguyen@example.com", Address = "Hà Nội", Type = CustomerType.Student, TotalPurchased = 4_500_000, Note = "Đang theo học lớp học online" },
                new CustomerDto { CustomerId = 12, CustomerName = "Trần Thị L", PhoneNumber = "0988990011", Email = "l.tran@example.com", Address = "TP.HCM", Type = CustomerType.Regular, TotalPurchased = 7_300_000, Note = "" },
                new CustomerDto { CustomerId = 13, CustomerName = "Phạm Minh M", PhoneNumber = "0999001122", Email = "m.pham@example.com", Address = "Đà Nẵng", Type = CustomerType.VIP, TotalPurchased = 120_000_000, Note = "Khách VIP thân thiết" },
                new CustomerDto { CustomerId = 14, CustomerName = "Lê Thị N", PhoneNumber = "0901122334", Email = "n.le@example.com", Address = "Hải Phòng", Type = CustomerType.Regular, TotalPurchased = 6_500_000, Note = "" },
                new CustomerDto { CustomerId = 15, CustomerName = "Vũ Văn O", PhoneNumber = "0912233445", Email = "o.vu@example.com", Address = "Cần Thơ", Type = CustomerType.Student, TotalPurchased = 3_200_000, Note = "" },
                new CustomerDto { CustomerId = 16, CustomerName = "Ngô Thị P", PhoneNumber = "0923344556", Email = "p.ngo@example.com", Address = "Hà Nội", Type = CustomerType.VIP, TotalPurchased = 80_000_000, Note = "Khách VIP ưu tiên" },
                new CustomerDto { CustomerId = 17, CustomerName = "Đặng Văn Q", PhoneNumber = "0934455667", Email = "q.dang@example.com", Address = "TP.HCM", Type = CustomerType.Regular, TotalPurchased = 9_800_000, Note = "" },
                new CustomerDto { CustomerId = 18, CustomerName = "Phan Thị R", PhoneNumber = "0945566778", Email = "r.phan@example.com", Address = "Đà Nẵng", Type = CustomerType.Student, TotalPurchased = 2_500_000, Note = "" },
                new CustomerDto { CustomerId = 19, CustomerName = "Trương Minh S", PhoneNumber = "0956677889", Email = "s.truong@example.com", Address = "Hải Phòng", Type = CustomerType.VIP, TotalPurchased = 100_000_000, Note = "Khách VIP thân thiết" },
                new CustomerDto { CustomerId = 20, CustomerName = "Bùi Thị T", PhoneNumber = "0967788990", Email = "t.bui@example.com", Address = "Cần Thơ", Type = CustomerType.Regular, TotalPurchased = 5_500_000, Note = "" }
            };
        }

        // Lấy tất cả khách hàng
        public Task<ResponseWrapper<List<CustomerDto>>> GetAllCustomersAsync()
        {
            var response = new ResponseWrapper<List<CustomerDto>>
            {
                Success = true,
                Message = "Customers retrieved successfully",
                Data = _mockCustomers
            };
            return Task.FromResult(response);
        }

        // Lấy khách hàng theo ID
        public Task<ResponseWrapper<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            var customer = _mockCustomers.FirstOrDefault(c => c.CustomerId == id);
            var response = new ResponseWrapper<CustomerDto>
            {
                Success = customer != null,
                Message = customer != null ? "Customer retrieved successfully" : "Customer not found",
                Data = customer
            };
            return Task.FromResult(response);
        }

        // Tạo khách hàng mới
        public Task<ResponseWrapper<CustomerDto>> CreateCustomerAsync(CustomerUpsertRequestDto dto)
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

            var response = new ResponseWrapper<CustomerDto>
            {
                Success = true,
                Message = "Customer created successfully",
                Data = newCustomer
            };
            return Task.FromResult(response);
        }

        // Cập nhật thông tin khách hàng
        public Task<ResponseWrapper<CustomerDto>> UpdateCustomerAsync(int id, CustomerUpsertRequestDto dto)
        {
            var existing = _mockCustomers.FirstOrDefault(c => c.CustomerId == id);
            if (existing != null)
            {
                existing.CustomerName = dto.CustomerName;
                existing.PhoneNumber = dto.PhoneNumber;
                existing.Email = dto.Email;
                existing.Address = dto.Address;
                existing.Type = dto.Type;
                existing.Note = dto.Note;
            }

            var response = new ResponseWrapper<CustomerDto>
            {
                Success = existing != null,
                Message = existing != null ? "Customer updated successfully" : "Customer not found",
                Data = existing
            };
            return Task.FromResult(response);
        }

        // Xoá khách hàng
        public Task<ResponseWrapper<bool>> DeleteCustomerAsync(int id)
        {
            var existing = _mockCustomers.FirstOrDefault(c => c.CustomerId == id);
            bool success = false;

            if (existing != null)
            {
                _mockCustomers.Remove(existing);
                success = true;
            }

            var response = new ResponseWrapper<bool>
            {
                Success = success,
                Message = success ? "Customer deleted successfully" : "Customer not found",
                Data = success
            };
            return Task.FromResult(response);
        }

        // Lọc & sắp xếp (CustomerQueryDto)
        public Task<ResponseWrapper<List<CustomerDto>>> QueryCustomersAsync(CustomerListQueryDto query)
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

            var response = new ResponseWrapper<List<CustomerDto>>
            {
                Success = true,
                Message = "Customers queried successfully",
                Data = result.ToList()
            };
            return Task.FromResult(response);
        }
    }
}
