using FluentAssertions;
using VaultGuard.Domain.Entities;

namespace VaultGuard.Domain.Tests.Entities;

public sealed class DeviceTests
{
    [Fact]
    public void Create_ShouldCreateDevice_WhenValidParametersProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceInfo = "MacBook Pro - Mozilla/5.0";

        // Act
        var device = Device.Create(userId, deviceInfo);

        // Assert
        device.Should().NotBeNull();
        device.Id.Should().NotBeEmpty();
        device.UserId.Should().Be(userId);
        device.DeviceInfo.Should().Be(deviceInfo);
        device.IsActive.Should().BeTrue();
        device.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        device.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = Guid.Empty;
        var deviceInfo = "Device Info";

        // Act
        var act = () => Device.Create(userId, deviceInfo);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UserId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenDeviceInfoIsNullOrEmpty(string? deviceInfo)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => Device.Create(userId, deviceInfo!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*DeviceInfo*");
    }

    [Fact]
    public void UpdateLastAccess_ShouldUpdateLastAccessedAt()
    {
        // Arrange
        var device = CreateDefaultDevice();
        var initialLastAccess = device.LastAccessedAt;

        Thread.Sleep(10); // Small delay to ensure different timestamp

        // Act
        device.UpdateLastAccess();

        // Assert
        device.LastAccessedAt.Should().BeAfter(initialLastAccess);
        device.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateLastAccess_ShouldUpdateOnMultipleCalls()
    {
        // Arrange
        var device = CreateDefaultDevice();
        
        // Act
        device.UpdateLastAccess();
        var firstAccess = device.LastAccessedAt;
        
        Thread.Sleep(10);
        device.UpdateLastAccess();
        var secondAccess = device.LastAccessedAt;

        // Assert
        secondAccess.Should().BeAfter(firstAccess);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var device = CreateDefaultDevice();

        // Act
        device.Deactivate();

        // Assert
        device.IsActive.Should().BeFalse();
        device.UpdatedAt.Should().NotBeNull();
        device.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var device = CreateDefaultDevice();
        device.Deactivate();

        // Act
        device.Activate();

        // Assert
        device.IsActive.Should().BeTrue();
        device.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldAllowMultipleCalls()
    {
        // Arrange
        var device = CreateDefaultDevice();

        // Act
        device.Deactivate();
        device.Deactivate();

        // Assert
        device.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldAllowMultipleCalls()
    {
        // Arrange
        var device = CreateDefaultDevice();

        // Act
        device.Activate();
        device.Activate();

        // Assert
        device.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("Desktop - Windows 11")]
    [InlineData("Mobile - iPhone 15")]
    [InlineData("Tablet - iPad Pro")]
    [InlineData("Browser - Chrome")]
    public void Create_ShouldSupportVariousDeviceInfo(string deviceInfo)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var device = Device.Create(userId, deviceInfo);

        // Assert
        device.DeviceInfo.Should().Be(deviceInfo);
    }

    [Fact]
    public void ActivationToggle_ShouldWorkCorrectly()
    {
        // Arrange
        var device = CreateDefaultDevice();

        // Act & Assert
        device.IsActive.Should().BeTrue();
        
        device.Deactivate();
        device.IsActive.Should().BeFalse();
        
        device.Activate();
        device.IsActive.Should().BeTrue();
        
        device.Deactivate();
        device.IsActive.Should().BeFalse();
        
        device.Activate();
        device.IsActive.Should().BeTrue();
    }

    private static Device CreateDefaultDevice()
    {
        return Device.Create(
            Guid.NewGuid(),
            "Test Device - Browser/5.0");
    }
}
