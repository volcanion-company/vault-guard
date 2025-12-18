using FluentAssertions;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;

namespace VaultGuard.Domain.Tests.Entities;

public sealed class AuditLogTests
{
    [Fact]
    public void Create_ShouldCreateAuditLog_WhenAllParametersProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var action = AuditAction.VaultCreated;
        var metadata = "{\"name\":\"My Vault\"}";
        var ipAddress = "192.168.1.100";
        var userAgent = "Mozilla/5.0";

        // Act
        var auditLog = AuditLog.Create(
            userId,
            action,
            metadata,
            ipAddress,
            userAgent);

        // Assert
        auditLog.Should().NotBeNull();
        auditLog.Id.Should().NotBeEmpty();
        auditLog.UserId.Should().Be(userId);
        auditLog.Action.Should().Be(action);
        auditLog.Metadata.Should().Be(metadata);
        auditLog.IpAddress.Should().Be(ipAddress);
        auditLog.UserAgent.Should().Be(userAgent);
        auditLog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldCreateAuditLog_WhenOptionalParametersAreNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var action = AuditAction.VaultDeleted;

        // Act
        var auditLog = AuditLog.Create(userId, action);

        // Assert
        auditLog.Should().NotBeNull();
        auditLog.Metadata.Should().BeNull();
        auditLog.IpAddress.Should().BeNull();
        auditLog.UserAgent.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;
        var action = AuditAction.VaultCreated;

        // Act
        var act = () => AuditLog.Create(userId, action);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UserId*");
    }

    [Theory]
    [InlineData(AuditAction.VaultCreated)]
    [InlineData(AuditAction.VaultUpdated)]
    [InlineData(AuditAction.VaultDeleted)]
    [InlineData(AuditAction.VaultItemCreated)]
    [InlineData(AuditAction.VaultItemUpdated)]
    [InlineData(AuditAction.VaultItemDeleted)]
    [InlineData(AuditAction.VaultItemViewed)]
    [InlineData(AuditAction.VaultShared)]
    [InlineData(AuditAction.LoginSuccess)]
    [InlineData(AuditAction.LoginFailed)]
    [InlineData(AuditAction.PasswordChanged)]
    [InlineData(AuditAction.DeviceRegistered)]
    public void Create_ShouldSupportAllAuditActions(AuditAction action)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var auditLog = AuditLog.Create(userId, action);

        // Assert
        auditLog.Action.Should().Be(action);
    }

    [Fact]
    public void PrivateConstructor_ShouldBeCallableViaReflection()
    {
        // Arrange
        var constructor = typeof(AuditLog).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            Type.EmptyTypes,
            null);

        // Act
        var auditLog = constructor?.Invoke(null) as AuditLog;

        // Assert
        auditLog.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldStoreMetadataAsJson()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var action = AuditAction.VaultItemCreated;
        var metadata = "{\"type\":\"Password\",\"name\":\"Gmail Account\"}";

        // Act
        var auditLog = AuditLog.Create(userId, action, metadata);

        // Assert
        auditLog.Metadata.Should().Be(metadata);
    }

    [Fact]
    public void Create_ShouldStoreIpAddressAndUserAgent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var action = AuditAction.LoginSuccess;
        var ipAddress = "203.0.113.42";
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

        // Act
        var auditLog = AuditLog.Create(
            userId,
            action,
            null,
            ipAddress,
            userAgent);

        // Assert
        auditLog.IpAddress.Should().Be(ipAddress);
        auditLog.UserAgent.Should().Be(userAgent);
    }

    [Fact]
    public void AuditLog_ShouldBeImmutable()
    {
        // Arrange & Act
        var auditLog = AuditLog.Create(
            Guid.NewGuid(),
            AuditAction.VaultCreated);

        // Assert - Verify there are no public setters
        var properties = typeof(AuditLog).GetProperties()
            .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true);
        
        properties.Should().BeEmpty("AuditLog should be immutable after creation");
    }

    [Fact]
    public void Create_ShouldWorkWithVaultActions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var createLog = AuditLog.Create(userId, AuditAction.VaultCreated);
        var updateLog = AuditLog.Create(userId, AuditAction.VaultUpdated);
        var deleteLog = AuditLog.Create(userId, AuditAction.VaultDeleted);

        // Assert
        createLog.Action.Should().Be(AuditAction.VaultCreated);
        updateLog.Action.Should().Be(AuditAction.VaultUpdated);
        deleteLog.Action.Should().Be(AuditAction.VaultDeleted);
    }

    [Fact]
    public void Create_ShouldWorkWithVaultItemActions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var createLog = AuditLog.Create(userId, AuditAction.VaultItemCreated);
        var viewedLog = AuditLog.Create(userId, AuditAction.VaultItemViewed);

        // Assert
        createLog.Action.Should().Be(AuditAction.VaultItemCreated);
        viewedLog.Action.Should().Be(AuditAction.VaultItemViewed);
    }

    [Fact]
    public void Create_ShouldWorkWithDeviceAction()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var registerLog = AuditLog.Create(userId, AuditAction.DeviceRegistered);

        // Assert
        registerLog.Action.Should().Be(AuditAction.DeviceRegistered);
    }
}
