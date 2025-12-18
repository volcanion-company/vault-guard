using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.Features.Vaults.Commands;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.Repositories;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Application.Tests.Features.Vaults;

public sealed class CreateVaultCommandHandlerTests
{
    private readonly Mock<IVaultRepository> _vaultRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<CreateVaultCommandHandler>> _loggerMock;
    private readonly CreateVaultCommandHandler _handler;

    public CreateVaultCommandHandlerTests()
    {
        _vaultRepositoryMock = new Mock<IVaultRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<CreateVaultCommandHandler>>();

        _handler = new CreateVaultCommandHandler(
            _vaultRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateVault_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateVaultCommand
        {
            Name = "Test Vault",
            EncryptedVaultKeyCipherText = "encrypted_data_cipher",
            EncryptedVaultKeyIV = "iv_data"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IpAddress).Returns("127.0.0.1");
        _currentUserServiceMock.Setup(x => x.UserAgent).Returns("Test Agent");

        _vaultRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vault v, CancellationToken ct) => v);

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLog a, CancellationToken ct) => a);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Vault");
        result.Version.Should().Be(1);
        result.ItemCount.Should().Be(0);

        _vaultRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditLogRepositoryMock.Verify(x => x.AddAsync(It.Is<AuditLog>(a => a.Action == AuditAction.VaultCreated), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPrefixAsync($"vaults:{userId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateAuditLog_WithCorrectInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";
        var command = new CreateVaultCommand
        {
            Name = "Test Vault",
            EncryptedVaultKeyCipherText = "cipher",
            EncryptedVaultKeyIV = "iv"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IpAddress).Returns(ipAddress);
        _currentUserServiceMock.Setup(x => x.UserAgent).Returns(userAgent);

        _vaultRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vault v, CancellationToken ct) => v);

        AuditLog? capturedAuditLog = null;
        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((a, ct) => capturedAuditLog = a)
            .ReturnsAsync((AuditLog a, CancellationToken ct) => a);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedAuditLog.Should().NotBeNull();
        capturedAuditLog!.UserId.Should().Be(userId);
        capturedAuditLog.Action.Should().Be(AuditAction.VaultCreated);
        capturedAuditLog.IpAddress.Should().Be(ipAddress);
        capturedAuditLog.UserAgent.Should().Be(userAgent);
        capturedAuditLog.Metadata.Should().Contain("Test Vault");
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCache_AfterVaultCreation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateVaultCommand
        {
            Name = "Test Vault",
            EncryptedVaultKeyCipherText = "cipher",
            EncryptedVaultKeyIV = "iv"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _vaultRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vault v, CancellationToken ct) => v);

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLog a, CancellationToken ct) => a);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cacheServiceMock.Verify(
            x => x.RemoveByPrefixAsync($"vaults:{userId}", It.IsAny<CancellationToken>()),
            Times.Once,
            "Cache should be invalidated after vault creation");
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenLogLevelEnabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateVaultCommand
        {
            Name = "Test Vault",
            EncryptedVaultKeyCipherText = "cipher",
            EncryptedVaultKeyIV = "iv"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IpAddress).Returns("127.0.0.1");
        _currentUserServiceMock.Setup(x => x.UserAgent).Returns("Test Agent");

        _vaultRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vault v, CancellationToken ct) => v);

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLog a, CancellationToken ct) => a);

        _loggerMock
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating vault for user {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("created successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotLogInformation_WhenLogLevelDisabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateVaultCommand
        {
            Name = "Test Vault",
            EncryptedVaultKeyCipherText = "cipher",
            EncryptedVaultKeyIV = "iv"
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _currentUserServiceMock.Setup(x => x.IpAddress).Returns("127.0.0.1");
        _currentUserServiceMock.Setup(x => x.UserAgent).Returns("Test Agent");

        _vaultRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Vault>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vault v, CancellationToken ct) => v);

        _auditLogRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLog a, CancellationToken ct) => a);

        _loggerMock
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.IsEnabled(LogLevel.Information),
            Times.Once);

        // Verify the conditional log is NOT called when IsEnabled returns false
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating vault for user {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
