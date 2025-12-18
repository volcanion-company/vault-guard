using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using VaultGuard.Api.Middleware;
using VaultGuard.Api.Services;
using VaultGuard.Application;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Infrastructure;
using VaultGuard.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog with environment-aware configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.Conditional(
        evt => !string.IsNullOrEmpty(context.Configuration["Elasticsearch:Uri"]),
        wt => wt.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
            new Uri(context.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            IndexFormat = $"vaultguard-logs-{context.HostingEnvironment.EnvironmentName.ToLower()}-{{0:yyyy.MM.dd}}",
            NumberOfShards = 2,
            NumberOfReplicas = 1
        })
    ));

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

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = !string.IsNullOrEmpty(issuer),
            ValidIssuer = issuer,
            ValidateAudience = !string.IsNullOrEmpty(audience),
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowConfiguredOrigins", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

// Add Health Checks
var writeDbConnection = builder.Configuration.GetConnectionString("WriteDatabase");
var redisConnection = builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrEmpty(writeDbConnection) && !string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddHealthChecks()
        .AddNpgSql(writeDbConnection)
        .AddRedis(redisConnection);
}
else if (builder.Environment.IsDevelopment())
{
    // In development, add basic health check even if connections not configured
    builder.Services.AddHealthChecks();
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply CORS if configured
if (allowedOrigins.Length > 0)
{
    app.UseCors("AllowConfiguredOrigins");
}

// Serilog request logging
app.UseSerilogRequestLogging();

// Request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks endpoint (if configured)
if (builder.Services.Any(s => s.ServiceType == typeof(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService)))
{
    app.MapHealthChecks("/health");
}

app.MapControllers();

Log.Information("VaultGuard API starting...");

app.Run();

Log.Information("VaultGuard API stopped");
Log.CloseAndFlush();
