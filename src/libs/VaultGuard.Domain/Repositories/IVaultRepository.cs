using VaultGuard.Domain.Entities;

namespace VaultGuard.Domain.Repositories;

/// <summary>
/// Repository interface for Vault aggregate
/// </summary>
public interface IVaultRepository
{
    Task<Vault?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Vault>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Vault> AddAsync(Vault vault, CancellationToken cancellationToken = default);
    void Update(Vault vault);
    void Delete(Vault vault);
}
