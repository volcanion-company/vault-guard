using VaultGuard.Domain.Events;

namespace VaultGuard.Domain.Tests.Events;

public class VaultItemCreatedEventTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Arrange
        var vaultItemId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new VaultItemCreatedEvent(vaultItemId, vaultId, userId);

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.Equal(vaultItemId, domainEvent.VaultItemId);
        Assert.Equal(vaultId, domainEvent.VaultId);
        Assert.Equal(userId, domainEvent.UserId);
        Assert.True(domainEvent.OccurredOn <= DateTime.UtcNow);
    }

    [Fact]
    public void VaultItemId_ShouldReturnCorrectValue()
    {
        // Arrange
        var vaultItemId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new VaultItemCreatedEvent(vaultItemId, vaultId, userId);

        // Assert
        Assert.Equal(vaultItemId, domainEvent.VaultItemId);
    }

    [Fact]
    public void VaultId_ShouldReturnCorrectValue()
    {
        // Arrange
        var vaultItemId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new VaultItemCreatedEvent(vaultItemId, vaultId, userId);

        // Assert
        Assert.Equal(vaultId, domainEvent.VaultId);
    }

    [Fact]
    public void UserId_ShouldReturnCorrectValue()
    {
        // Arrange
        var vaultItemId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new VaultItemCreatedEvent(vaultItemId, vaultId, userId);

        // Assert
        Assert.Equal(userId, domainEvent.UserId);
    }

    [Fact]
    public void Constructor_ShouldInheritFromBaseDomainEvent()
    {
        // Arrange
        var vaultItemId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new VaultItemCreatedEvent(vaultItemId, vaultId, userId);

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.Equal(DateTimeKind.Utc, domainEvent.OccurredOn.Kind);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var vaultItemId = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var domainEvent = new VaultItemCreatedEvent(vaultItemId, vaultId, userId);
        var vaultItemIdProperty = typeof(VaultItemCreatedEvent).GetProperty(nameof(VaultItemCreatedEvent.VaultItemId));
        var vaultIdProperty = typeof(VaultItemCreatedEvent).GetProperty(nameof(VaultItemCreatedEvent.VaultId));
        var userIdProperty = typeof(VaultItemCreatedEvent).GetProperty(nameof(VaultItemCreatedEvent.UserId));

        // Assert
        Assert.Null(vaultItemIdProperty?.SetMethod);
        Assert.Null(vaultIdProperty?.SetMethod);
        Assert.Null(userIdProperty?.SetMethod);
    }
}

