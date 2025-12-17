using Microsoft.EntityFrameworkCore;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Repositories;
using VaultGuard.Persistence.Contexts;

namespace VaultGuard.Persistence.Repositories;

public sealed class VaultRepository : IVaultRepository
{
    private readonly WriteDbContext _writeContext;
    private readonly ReadDbContext _readContext;

    public VaultRepository(WriteDbContext writeContext, ReadDbContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }

    public async Task<Vault?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Query from read database for better performance
        return await _readContext.Vaults
            .Include(v => v.Items)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Vault>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // Query from read database
        return await _readContext.Vaults
            .Include(v => v.Items)
            .Where(v => v.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Vault> AddAsync(Vault vault, CancellationToken cancellationToken = default)
    {
        // Write to write database
        var entry = await _writeContext.Vaults.AddAsync(vault, cancellationToken);
        return entry.Entity;
    }

    public void Update(Vault vault)
    {
        // Write to write database
        _writeContext.Vaults.Update(vault);
    }

    public void Delete(Vault vault)
    {
        // Soft delete - just mark as deleted
        vault.Delete();
        _writeContext.Vaults.Update(vault);
    }
}
