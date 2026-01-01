using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using TechHaven.Application;
using TechHaven.Application.Interfaces;
using TechHaven.Infrastructure;
using TechHaven.Infrastructure.Data;
using TechHaven.Infrastructure.Persistence;
using TechHaven.Infrastructure.Services;
using TechHaven.Presentation.WebAPI.Middleware;
using TechHaven.Presentation.WebAPI.Seeders;
using TechHaven.Shared.DTOs.Common;

// Load environment variables FIRST
Env.Load();

// Configure Serilog with environment-aware settings
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var logLevel = environment == "Development" 
    ? Serilog.Events.LogEventLevel.Debug 
    : Serilog.Events.LogEventLevel.Information;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(logLevel)
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Environment", environment)
    .Enrich.WithProperty("Application", "TechHaven")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: environment == "Development" ? "logs/techhaven-.log" : "/app/logs/techhaven-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: environment == "Development" ? 7 : 30,
        fileSizeLimitBytes: 104857600, // 100MB
        rollOnFileSizeLimit: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting TechHaven API in {Environment} mode", environment);

    var builder = WebApplication.CreateBuilder(args);

    // Configure URLs
    var explicitUrl = Environment.GetEnvironmentVariable("TECHHAVEN_API_BASEURL") ?? "http://localhost:5207";
    builder.WebHost.UseUrls(explicitUrl);
    Log.Information("Configured URLs: {Url}", explicitUrl);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add configuration sources
    builder.Configuration.AddEnvironmentVariables();

    // Add services
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new
                    {
                        Field = e.Key,
                        Errors = e.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                    })
                    .ToList();

                var response = new ResponseWrapper<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Data = errors
                };

                return new BadRequestObjectResult(response);
            };
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "TechHaven API",
            Version = "v1",
            Description = "API for TechHaven Shop Management System"
        });

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<CellphoneProductSeeder>();
    builder.Services.AddScoped<OrderSeeder>();

    // Register Application & Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Global Exception Handler
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? Serilog.Events.LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? Serilog.Events.LogEventLevel.Error
                : Serilog.Events.LogEventLevel.Information;
    });

    // Database migration and seeding
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            await DbInitializer.SeedAsync(context, logger);
            // var productSeeder = services.GetRequiredService<CellphoneProductSeeder>();
            // await productSeeder.SeedAsync();
            var orderSeeder = services.GetRequiredService<OrderSeeder>();
            await orderSeeder.SeedAsync();
            Log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database seeding failed");
            throw;
        }
    }

    // Swagger (enabled in all environments for now, can be restricted later)
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    // 2. [MỚI] Đăng ký TenantMiddleware: Middleware này phải chạy SAU Authentication (để đọc được context.User) nhưng TRƯỚC Controllers (để kịp đổi DB).
    app.UseMiddleware<TenantMiddleware>();
    // 3. [MỚI] TrialBlockerMiddleware (Đặt sau Authentication để đọc được User)
    app.UseMiddleware<TrialBlockerMiddleware>();

    app.UseAuthorization();
    app.MapControllers();

    var urls = builder.WebHost.GetSetting("urls") ?? explicitUrl;
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