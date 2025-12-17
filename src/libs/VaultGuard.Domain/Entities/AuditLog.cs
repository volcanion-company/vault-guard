using VaultGuard.Domain.Common;
using VaultGuard.Domain.Enums;

namespace VaultGuard.Domain.Entities;

/// <summary>
/// AuditLog entity - tracks all sensitive operations
/// </summary>
public sealed class AuditLog : BaseEntity
{
    public Guid UserId { get; private set; }
    public AuditAction Action { get; private set; }
    public string? Metadata { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private AuditLog() { } // EF Core

    private AuditLog(Guid userId, AuditAction action, string? metadata, string? ipAddress, string? userAgent) : base()
    {
        UserId = userId;
        Action = action;
        Metadata = metadata;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public static AuditLog Create(Guid userId, AuditAction action, string? metadata = null, string? ipAddress = null, string? userAgent = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        return new AuditLog(userId, action, metadata, ipAddress, userAgent);
    }
}
