using TechHaven.Application.Interfaces;

namespace TechHaven.Infrastructure.Services;

/// <summary>
/// A Specific class that manages tenant-specific connection strings for dynamic database configuration.
/// </summary>
//Chúng ta cần một dịch vụ có vòng đời là Scoped (sống trong 1 request) để chứa connection string mà người dùng gửi lên
public class TenantService : ITenantService
{
    public string? ConnectionString { get; set; }

    public void SetConnectionString(string host, string port, string db, string user, string pass)
    {
        // Build PostgreSQL connection string
        ConnectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};";
    }
    public void SetConnectionString(string connectionString)
    {
        // Set the ConnectionString directly
        ConnectionString = connectionString;
    }
}