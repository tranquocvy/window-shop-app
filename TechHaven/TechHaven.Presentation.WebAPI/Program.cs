using TechHaven.Application;
using TechHaven.Infrastructure;
using TechHaven.Infrastructure.Data;
using TechHaven.Infrastructure.Persistence;
using DotNetEnv;

// Load environment variables FIRST, before creating builder
Env.Load();

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddHttpContextAccessor();

// Register Application & Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
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

app.Run();