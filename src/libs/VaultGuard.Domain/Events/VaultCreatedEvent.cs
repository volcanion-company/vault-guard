using VaultGuard.Domain.Common;

namespace VaultGuard.Domain.Events;

/// <summary>
/// Domain event raised when a vault is created
/// </summary>
public sealed class VaultCreatedEvent : BaseDomainEvent
{
    public Guid VaultId { get; }
    public Guid OwnerId { get; }
    public string VaultName { get; }

    public VaultCreatedEvent(Guid vaultId, Guid ownerId, string vaultName)
    {
        VaultId = vaultId;
        OwnerId = ownerId;
        VaultName = vaultName;
    }
}
