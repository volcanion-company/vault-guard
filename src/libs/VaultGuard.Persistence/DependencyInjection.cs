using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VaultGuard.Domain.Repositories;
using VaultGuard.Persistence.Contexts;
using VaultGuard.Persistence.Repositories;

namespace VaultGuard.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Write Database (Primary)
        services.AddDbContext<WriteDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("WriteDatabase"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(WriteDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(3);
                });
        });

        // Read Database (Replica)
        services.AddDbContext<ReadDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("ReadDatabase"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Register repositories
        services.AddScoped<IVaultRepository, VaultRepository>();
        services.AddScoped<IVaultItemRepository, VaultItemRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
