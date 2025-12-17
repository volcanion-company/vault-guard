using VaultGuard.Domain.Enums;

namespace VaultGuard.Application.DTOs;

public sealed class VaultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ItemCount { get; set; }
}

public class VaultItemDto
{
    public Guid Id { get; set; }
    public Guid VaultId { get; set; }
    public VaultItemType Type { get; set; }
    public string EncryptedPayload { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

public sealed class VaultItemDetailDto : VaultItemDto
{
    public string VaultName { get; set; } = string.Empty;
}

public sealed class AuditLogDto
{
    public Guid Id { get; set; }
    public AuditAction Action { get; set; }
    public string? Metadata { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
