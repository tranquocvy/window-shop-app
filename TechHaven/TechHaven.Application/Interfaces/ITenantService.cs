
namespace TechHaven.Application.Interfaces
{
    //Chúng ta cần một dịch vụ có vòng đời là Scoped (sống trong 1 request) để chứa connection string mà người dùng gửi lên.
    /// <summary>
    /// Indicate a service that manages tenant-specific connection strings for Dynamic Database Configuration.
    /// </summary>
    public interface ITenantService
    {
        string? ConnectionString { get; set; }
        void SetConnectionString(string host, string port, string db, string user, string pass);
        void SetConnectionString(string connectionString);
    }
}
