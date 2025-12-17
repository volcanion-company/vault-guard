using VaultGuard.Domain.Common;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Domain.Entities;

/// <summary>
/// VaultItem entity - represents an item stored in a vault (password, note, etc.)
/// </summary>
public sealed class VaultItem : BaseEntity
{
    public Guid VaultId { get; private set; }
    public VaultItemType Type { get; private set; }
    public EncryptedData EncryptedPayload { get; private set; }
    public string? Metadata { get; private set; }
    public int Version { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }

    // Navigation
    public Vault Vault { get; private set; } = null!;

    private VaultItem() { } // EF Core

    private VaultItem(Guid vaultId, VaultItemType type, EncryptedData encryptedPayload, string? metadata) : base()
    {
        VaultId = vaultId;
        Type = type;
        EncryptedPayload = encryptedPayload;
        Metadata = metadata;
        Version = 1;
        IsDeleted = false;
    }

    public static VaultItem Create(Guid vaultId, VaultItemType type, EncryptedData encryptedPayload, string? metadata = null)
    {
        if (vaultId == Guid.Empty)
            throw new ArgumentException("VaultId cannot be empty", nameof(vaultId));

        if (encryptedPayload == null)
            throw new ArgumentNullException(nameof(encryptedPayload));

        return new VaultItem(vaultId, type, encryptedPayload, metadata);
    }

    public void Update(EncryptedData newEncryptedPayload, string? newMetadata = null)
    {
        if (newEncryptedPayload == null)
            throw new ArgumentNullException(nameof(newEncryptedPayload));

        if (IsDeleted)
            throw new InvalidOperationException("Cannot update a deleted item");

        EncryptedPayload = newEncryptedPayload;
        Metadata = newMetadata;
        Version++;
        MarkAsUpdated();
    }

    public void Delete()
    {
        IsDeleted = true;
        MarkAsUpdated();
    }

    public void MarkAsAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
    }
}
