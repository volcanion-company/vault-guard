using VaultGuard.Domain.Events;

namespace VaultGuard.Domain.Tests.Common;

public class BaseDomainEventTests
{
    [Fact]
    public void BaseDomainEvent_ShouldInitializePropertiesCorrectly()
    {
        // Arrange & Act
        var domainEvent = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test Vault");

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.True((DateTime.UtcNow - domainEvent.OccurredOn).TotalSeconds < 5); // Ensure occurredOn is recent
    }

    [Fact]
    public void BaseDomainEvent_ShouldGenerateUniqueEventId()
    {
        // Arrange & Act
        var event1 = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Vault 1");
        var event2 = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Vault 2");

        // Assert
        Assert.NotEqual(event1.EventId, event2.EventId);
    }

    [Fact]
    public void BaseDomainEvent_EventId_ShouldNotBeEmpty()
    {
        // Arrange & Act
        var domainEvent = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test Vault");

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
    }

    [Fact]
    public void BaseDomainEvent_OccurredOn_ShouldBeUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var domainEvent = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test Vault");
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(domainEvent.OccurredOn >= beforeCreation);
        Assert.True(domainEvent.OccurredOn <= afterCreation);
        Assert.Equal(DateTimeKind.Utc, domainEvent.OccurredOn.Kind);
    }

    [Fact]
    public void BaseDomainEvent_OccurredOn_ShouldNotBeFutureTime()
    {
        // Arrange & Act
        var domainEvent = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test Vault");

        // Assert
        Assert.True(domainEvent.OccurredOn <= DateTime.UtcNow);
    }

    [Fact]
    public void BaseDomainEvent_MultipleInstances_ShouldHaveDistinctEventIds()
    {
        // Arrange & Act
        var events = new List<VaultCreatedEvent>();
        for (int i = 0; i < 100; i++)
        {
            events.Add(new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), $"Vault {i}"));
        }

        // Assert
        var uniqueEventIds = events.Select(e => e.EventId).Distinct().ToList();
        Assert.Equal(100, uniqueEventIds.Count);
    }

    [Fact]
    public void BaseDomainEvent_OccurredOn_ShouldBeCloseToCreationTime()
    {
        // Arrange
        var expectedTime = DateTime.UtcNow;

        // Act
        var domainEvent = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test Vault");

        // Assert
        var timeDifference = Math.Abs((domainEvent.OccurredOn - expectedTime).TotalMilliseconds);
        Assert.True(timeDifference < 1000); // Should be within 1 second
    }

    [Fact]
    public void BaseDomainEvent_Properties_ShouldBeReadOnly()
    {
        // Arrange & Act
        var domainEvent = new VaultCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Test Vault");
        var initialEventId = domainEvent.EventId;
        var initialOccurredOn = domainEvent.OccurredOn;

        // Simulate time passing
        System.Threading.Thread.Sleep(10);

        // Assert - Properties should not change
        Assert.Equal(initialEventId, domainEvent.EventId);
        Assert.Equal(initialOccurredOn, domainEvent.OccurredOn);
    }
}
