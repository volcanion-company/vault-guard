using VaultGuard.Domain.Common;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Domain.Entities;

/// <summary>
/// Vault aggregate root - represents a password vault
/// </summary>
public sealed class Vault : BaseEntity, IAggregateRoot
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public EncryptedData EncryptedVaultKey { get; private set; }
    public int Version { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation properties
    private readonly List<VaultItem> _items = new();
    public IReadOnlyCollection<VaultItem> Items => _items.AsReadOnly();

    private Vault() { } // EF Core

    private Vault(Guid ownerId, string name, EncryptedData encryptedVaultKey) : base()
    {
        OwnerId = ownerId;
        Name = name;
        EncryptedVaultKey = encryptedVaultKey;
        Version = 1;
        IsDeleted = false;
    }

    public static Vault Create(Guid ownerId, string name, EncryptedData encryptedVaultKey)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("OwnerId cannot be empty", nameof(ownerId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (encryptedVaultKey == null)
            throw new ArgumentNullException(nameof(encryptedVaultKey));

        return new Vault(ownerId, name, encryptedVaultKey);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));

        Name = newName;
        Version++;
        MarkAsUpdated();
    }

    public void Delete()
    {
        IsDeleted = true;
        MarkAsUpdated();
    }

    public VaultItem AddItem(VaultItemType type, EncryptedData encryptedPayload, string? metadata = null)
    {
        var item = VaultItem.Create(Id, type, encryptedPayload, metadata);
        _items.Add(item);
        Version++;
        MarkAsUpdated();
        return item;
    }

    public void RemoveItem(VaultItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);
        if (existingItem != null)
        {
            existingItem.Delete();
            Version++;
            MarkAsUpdated();
        }
    }

    public void EnsureOwnership(Guid userId)
    {
        if (OwnerId != userId)
            throw new UnauthorizedAccessException("User does not own this vault");
    }
}
