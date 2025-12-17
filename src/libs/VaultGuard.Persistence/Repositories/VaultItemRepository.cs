using Microsoft.EntityFrameworkCore;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Repositories;
using VaultGuard.Persistence.Contexts;

namespace VaultGuard.Persistence.Repositories;

public sealed class VaultItemRepository : IVaultItemRepository
{
    private readonly WriteDbContext _writeContext;
    private readonly ReadDbContext _readContext;

    public VaultItemRepository(WriteDbContext writeContext, ReadDbContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }

    public async Task<VaultItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _readContext.VaultItems
            .FirstOrDefaultAsync(vi => vi.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<VaultItem>> GetByVaultIdAsync(Guid vaultId, CancellationToken cancellationToken = default)
    {
        return await _readContext.VaultItems
            .Where(vi => vi.VaultId == vaultId)
            .ToListAsync(cancellationToken);
    }

    public async Task<VaultItem> AddAsync(VaultItem vaultItem, CancellationToken cancellationToken = default)
    {
        var entry = await _writeContext.VaultItems.AddAsync(vaultItem, cancellationToken);
        return entry.Entity;
    }

    public void Update(VaultItem vaultItem)
    {
        _writeContext.VaultItems.Update(vaultItem);
    }

    public void Delete(VaultItem vaultItem)
    {
        vaultItem.Delete();
        _writeContext.VaultItems.Update(vaultItem);
    }
}
