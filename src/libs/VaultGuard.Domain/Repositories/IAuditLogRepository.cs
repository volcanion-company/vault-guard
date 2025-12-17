using VaultGuard.Domain.Entities;

namespace VaultGuard.Domain.Repositories;

/// <summary>
/// Repository interface for AuditLog entity
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
