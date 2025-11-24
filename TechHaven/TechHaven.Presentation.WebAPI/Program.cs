using TechHaven.Application;
using TechHaven.Infrastructure;
using TechHaven.Infrastructure.Data;
using TechHaven.Infrastructure.Persistence;
using DotNetEnv;
using Serilog;

// ============================================
// Serilog Configuration Guide
// ============================================
// Logging Levels (từ thấp đến cao):
// - Verbose: Chi tiết cực kỳ nhiều, chỉ dùng khi debug sâu
// - Debug: Thông tin debug, không dùng trong production
// - Information: Luồng bình thường của app (API calls, database queries)
// - Warning: Vấn đề không nghiêm trọng nhưng cần chú ý
// - Error: Lỗi xảy ra nhưng app vẫn chạy được
// - Fatal: Lỗi nghiêm trọng khiến app crash
// ============================================

// Load environment variables FIRST, before creating builder
Env.Load();

// Configure Serilog BEFORE creating builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/techhaven-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting TechHaven API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add environment variables to configuration BEFORE registering services
    builder.Configuration.AddEnvironmentVariables();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "TechHaven API",
            Version = "v1",
            Description = "API for TechHaven Store Management System"
        });
    });

    // Register Application & Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Add request logging middleware
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // Seed the database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await DbInitializer.SeedAsync(context);
            Log.Information("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database seeding failed");
            throw;
        }
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // IMPORTANT: Authentication must come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Log the server URLs
    // foreach (var url in app.Urls)
    // {
    //     Log.Information("Server is running at {Url}", url);
    // }
    // Log the server URLs
    var urls = builder.WebHost.GetSetting("urls") ?? "http://localhost:5207";
    Log.Information("Server is running at {Urls}", urls);

    Log.Information("TechHaven API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}