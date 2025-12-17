using VaultGuard.Domain.Common;

namespace VaultGuard.Domain.Events;

/// <summary>
/// Domain event raised when a vault item is created
/// </summary>
public sealed class VaultItemCreatedEvent : BaseDomainEvent
{
    public Guid VaultItemId { get; }
    public Guid VaultId { get; }
    public Guid UserId { get; }

    public VaultItemCreatedEvent(Guid vaultItemId, Guid vaultId, Guid userId)
    {
        VaultItemId = vaultItemId;
        VaultId = vaultId;
        UserId = userId;
    }
}
