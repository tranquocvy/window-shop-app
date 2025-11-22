using Microsoft.EntityFrameworkCore.Storage;
using TechHaven.Domain.Interfaces;
using TechHaven.Infrastructure.Persistence.Repositories;

namespace TechHaven.Infrastructure.Persistence;

/// <summary>
/// Implementation of the Unit of Work pattern.
/// Coordinates multiple repositories and manages a single database transaction.
/// All repositories share the same DbContext instance, ensuring consistency.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    public IUserRepository Users => throw new NotImplementedException();

    public IRoleRepository Roles => throw new NotImplementedException();

    public IProductRepository Products => throw new NotImplementedException();

    public ICategoryRepository Categories => throw new NotImplementedException();

    public ICustomerRepository Customers => throw new NotImplementedException();

    public IOrderRepository Orders => throw new NotImplementedException();

    public IPaymentRepository Payments => throw new NotImplementedException();

    public ICommissionRepository Commissions => throw new NotImplementedException();

    public IAppSettingRepository AppSettings => throw new NotImplementedException();

        //1 transaction: định nghĩa
        //1.1 Thực hiện các thao tác liên quan đến quy trình - liên kết các repos với nhau - sử dụng nhiều repos cùng lúc
        //1.2 VD: Tạo mới một Order
        // -> Tạo customer
        // -> Tạo order detail
        // Có nghĩa là call tới nhiều repos khác nhau
        //Nếu như 1 lời gọi repos bị thất bại thì -> Transaction bị thất bại
        // Khi bị thất bại -> Call rollback để hoàn tác các thủ tục/ tác vụ đã dùng để tránh lưu data lỗi
        //Khi thành công transaction thì lưu.

        //1.3 Đôi lúc ko cần dùng begin transaction
        // -> NẾu như chỉ có 1 repos được sử dụng
        // UnitOFWork.Users.AddAsync(user)
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}