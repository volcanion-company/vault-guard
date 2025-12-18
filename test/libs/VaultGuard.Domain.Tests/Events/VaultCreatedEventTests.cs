using VaultGuard.Domain.Events;

namespace VaultGuard.Domain.Tests.Events;

public class VaultCreatedEventTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var vaultName = "My Secure Vault";

        // Act
        var domainEvent = new VaultCreatedEvent(vaultId, ownerId, vaultName);

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.Equal(vaultId, domainEvent.VaultId);
        Assert.Equal(ownerId, domainEvent.OwnerId);
        Assert.Equal(vaultName, domainEvent.VaultName);
        Assert.True(domainEvent.OccurredOn <= DateTime.UtcNow);
    }

    [Fact]
    public void VaultId_ShouldReturnCorrectValue()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var vaultName = "Test Vault";

        // Act
        var domainEvent = new VaultCreatedEvent(vaultId, ownerId, vaultName);

        // Assert
        Assert.Equal(vaultId, domainEvent.VaultId);
    }

    [Fact]
    public void OwnerId_ShouldReturnCorrectValue()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var vaultName = "Test Vault";

        // Act
        var domainEvent = new VaultCreatedEvent(vaultId, ownerId, vaultName);

        // Assert
        Assert.Equal(ownerId, domainEvent.OwnerId);
    }

    [Fact]
    public void VaultName_ShouldReturnCorrectValue()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var vaultName = "My Vault Name";

        // Act
        var domainEvent = new VaultCreatedEvent(vaultId, ownerId, vaultName);

        // Assert
        Assert.Equal(vaultName, domainEvent.VaultName);
    }

    [Fact]
    public void Constructor_ShouldInheritFromBaseDomainEvent()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var vaultName = "Test Vault";

        // Act
        var domainEvent = new VaultCreatedEvent(vaultId, ownerId, vaultName);

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.Equal(DateTimeKind.Utc, domainEvent.OccurredOn.Kind);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var vaultName = "Test Vault";

        // Act
        var domainEvent = new VaultCreatedEvent(vaultId, ownerId, vaultName);
        var vaultIdProperty = typeof(VaultCreatedEvent).GetProperty(nameof(VaultCreatedEvent.VaultId));
        var ownerIdProperty = typeof(VaultCreatedEvent).GetProperty(nameof(VaultCreatedEvent.OwnerId));
        var vaultNameProperty = typeof(VaultCreatedEvent).GetProperty(nameof(VaultCreatedEvent.VaultName));

        // Assert
        Assert.Null(vaultIdProperty?.SetMethod);
        Assert.Null(ownerIdProperty?.SetMethod);
        Assert.Null(vaultNameProperty?.SetMethod);
    }
}

