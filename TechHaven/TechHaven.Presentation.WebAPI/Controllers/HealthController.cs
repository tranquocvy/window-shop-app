using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechHaven.Infrastructure.Persistence;

namespace TechHaven.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        AppDbContext context,
        ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var response = new HealthCheckResponse
            {
                Status = "healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetVersion(),
                Environment = GetEnvironment(),
                Uptime = GetUptime()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");

            var response = new HealthCheckResponse
            {
                Status = "unhealthy",
                Timestamp = DateTime.UtcNow,
                Version = GetVersion(),
                Environment = GetEnvironment(),
                Error = ex.Message
            };

            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
    }

    /// <summary>
    /// Detailed health check with database connection test
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(DetailedHealthCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DetailedHealthCheckResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDetailed()
    {
        var response = new DetailedHealthCheckResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetVersion(),
            Environment = GetEnvironment(),
            Uptime = GetUptime(),
            Checks = new Dictionary<string, ComponentHealth>()
        };

        // Check Database
        var dbHealth = await CheckDatabaseAsync();
        response.Checks["database"] = dbHealth;

        // Check Supabase Storage (optional)
        var storageHealth = CheckStorage();
        response.Checks["storage"] = storageHealth;

        // Determine overall status
        var isHealthy = response.Checks.All(c => c.Value.Status == "healthy");
        response.Status = isHealthy ? "healthy" : "degraded";

        var statusCode = isHealthy 
            ? StatusCodes.Status200OK 
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Check if API is ready to accept requests
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // Test database connection
            await _context.Database.CanConnectAsync();
            
            return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Readiness check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                new { status = "not ready", error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Check if API is alive (for load balancer)
    /// </summary>
    [HttpGet("alive")]
    public IActionResult Alive()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    // Helper Methods
    private async Task<ComponentHealth> CheckDatabaseAsync()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _context.Database.CanConnectAsync();
            
            // Get database statistics
            var productCount = await _context.Products.CountAsync();
            var orderCount = await _context.Orders.CountAsync();
            var customerCount = await _context.Customers.CountAsync();

            stopwatch.Stop();

            return new ComponentHealth
            {
                Status = "healthy",
                ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                Details = new Dictionary<string, object>
                {
                    { "connected", true },
                    { "products", productCount },
                    { "orders", orderCount },
                    { "customers", customerCount }
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database health check failed");

            return new ComponentHealth
            {
                Status = "unhealthy",
                ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                Error = ex.Message,
                Details = new Dictionary<string, object>
                {
                    { "connected", false }
                }
            };
        }
    }

    private ComponentHealth CheckStorage()
    {
        try
        {
            var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
            var hasApiKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUPABASE_API_KEY"));

            return new ComponentHealth
            {
                Status = !string.IsNullOrEmpty(supabaseUrl) && hasApiKey ? "healthy" : "degraded",
                Details = new Dictionary<string, object>
                {
                    { "configured", !string.IsNullOrEmpty(supabaseUrl) && hasApiKey },
                    { "url", supabaseUrl ?? "not configured" }
                }
            };
        }
        catch (Exception ex)
        {
            return new ComponentHealth
            {
                Status = "unknown",
                Error = ex.Message
            };
        }
    }

    private string GetVersion()
    {
        var version = typeof(Program).Assembly.GetName().Version;
        return version?.ToString() ?? "unknown";
    }

    private string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    }

    private string GetUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
    }
}

// Response Models
public class HealthCheckResponse
{
    public string Status { get; set; } = "healthy";
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string? Uptime { get; set; }
    public string? Error { get; set; }
}

public class DetailedHealthCheckResponse : HealthCheckResponse
{
    public Dictionary<string, ComponentHealth> Checks { get; set; } = new();
}

public class ComponentHealth
{
    public string Status { get; set; } = "healthy";
    public string? ResponseTime { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}