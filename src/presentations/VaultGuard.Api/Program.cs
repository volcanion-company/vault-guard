using Serilog;
using VaultGuard.Api.Middleware;
using VaultGuard.Api.Services;
using VaultGuard.API.Middleware;
using VaultGuard.Application;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Infrastructure;
using VaultGuard.Persistence;

// Configure Serilog
VaultGuard.Infrastructure.DependencyInjection.ConfigureSerilog(
    new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build(),
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add HttpContextAccessor (required for CurrentUserService)
builder.Services.AddHttpContextAccessor();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "VaultGuard API", Version = "v1" });
});

// Add layers
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Add CurrentUserService (bridges HttpContext to Application layer)
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("WriteDatabase")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serilog request logging
app.UseSerilogRequestLogging();

// Auth middleware (provided by Auth Service)
app.UseMiddleware<JwtAuthenticationMiddleware>();

// Request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");

app.MapControllers();

Log.Information("VaultGuard API starting...");

app.Run();

Log.Information("VaultGuard API stopped");
Log.CloseAndFlush();
