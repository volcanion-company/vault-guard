using VaultGuard.Domain.Entities;

namespace VaultGuard.Domain.Repositories;

/// <summary>
/// Repository interface for VaultItem entity
/// </summary>
public interface IVaultItemRepository
{
    Task<VaultItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<VaultItem>> GetByVaultIdAsync(Guid vaultId, CancellationToken cancellationToken = default);
    Task<VaultItem> AddAsync(VaultItem vaultItem, CancellationToken cancellationToken = default);
    void Update(VaultItem vaultItem);
    void Delete(VaultItem vaultItem);
}
