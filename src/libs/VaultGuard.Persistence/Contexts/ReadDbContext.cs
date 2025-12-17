using Microsoft.EntityFrameworkCore;
using VaultGuard.Domain.Entities;

namespace VaultGuard.Persistence.Contexts;

/// <summary>
/// Read database context - Read replica for query operations
/// </summary>
public sealed class ReadDbContext : DbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vault> Vaults => Set<Vault>();
    public DbSet<VaultItem> VaultItems => Set<VaultItem>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly);

        // Read database is read-only (for queries)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entityType.ClrType).ToTable(entityType.GetTableName()!, t => t.ExcludeFromMigrations());
        }
    }
}
