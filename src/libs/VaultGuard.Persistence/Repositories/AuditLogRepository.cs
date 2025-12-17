using Microsoft.EntityFrameworkCore;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Repositories;
using VaultGuard.Persistence.Contexts;

namespace VaultGuard.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly WriteDbContext _writeContext;
    private readonly ReadDbContext _readContext;

    public AuditLogRepository(WriteDbContext writeContext, ReadDbContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        var entry = await _writeContext.AuditLogs.AddAsync(auditLog, cancellationToken);
        return entry.Entity;
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _readContext.AuditLogs
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
