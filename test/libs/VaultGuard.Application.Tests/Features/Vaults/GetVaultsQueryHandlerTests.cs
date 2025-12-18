using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.DTOs;
using VaultGuard.Application.Features.Vaults.Queries;
using VaultGuard.Domain.Repositories;

namespace VaultGuard.Application.Tests.Features.Vaults;

public class GetVaultsQueryHandlerTests
{
    private readonly Mock<IVaultRepository> mockVaultRepository = new();
    private readonly Mock<ICurrentUserService> mockCurrentUserService = new();
    private readonly Mock<ICacheService> mockCacheService = new();
    private readonly Mock<ILogger<GetVaultsQueryHandler>> mockLogger = new();
    private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
    private readonly GetVaultsQueryHandler handler;

    public GetVaultsQueryHandlerTests()
    {
        handler = new GetVaultsQueryHandler(
            mockVaultRepository.Object,
            mockCurrentUserService.Object,
            mockCacheService.Object,
            mockLogger.Object
        );
    }

    [Fact]
    public void Handler_Should_Be_Initializable()
    {
        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_Should_Return_From_Cache()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var cachedVaults = new List<VaultDto>
        {
            new VaultDto
            {
                Id = vaultId,
                Name = "Cached Vault",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedVaults);

        var request = new GetVaultsQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Cached Vault", result.First().Name);
    }

    [Fact]
    public async Task Handle_Should_Return_From_Repository_When_Cache_Miss()
    {
        // Arrange
        mockCacheService
            .Setup(x => x.GetAsync<List<VaultDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<VaultDto>?)null);

        var repoVaults = new List<Domain.Entities.Vault>
        {
            Domain.Entities.Vault.Create(
                Guid.NewGuid(),
                "Repo Vault",
                Domain.ValueObjects.EncryptedData.Create("ciphertext", "iv"))
        };

        mockVaultRepository
            .Setup(x => x.GetByOwnerIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoVaults);
        var request = new GetVaultsQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Repo Vault", result.First().Name);
    }

    [Fact]
    public async Task Handle_Should_Calculate_ItemCount_Correctly_Excluding_Deleted_Items()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vault = Domain.Entities.Vault.Create(
            ownerId,
            "Test Vault",
            Domain.ValueObjects.EncryptedData.Create("ciphertext", "iv"));

        // Add 3 items using domain method
        var item1 = vault.AddItem(
            Domain.Enums.VaultItemType.Password,
            Domain.ValueObjects.EncryptedData.Create("payload1", "iv1"),
            "metadata1");
        
        var item2 = vault.AddItem(
            Domain.Enums.VaultItemType.Password,
            Domain.ValueObjects.EncryptedData.Create("payload2", "iv2"),
            "metadata2");
        
        var item3 = vault.AddItem(
            Domain.Enums.VaultItemType.Document,
            Domain.ValueObjects.EncryptedData.Create("payload3", "iv3"),
            "metadata3");

        // Delete one item using domain method
        vault.RemoveItem(item3);

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<VaultDto>?)null);

        mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(ownerId);

        mockVaultRepository
            .Setup(x => x.GetByOwnerIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Vault> { vault });

        var request = new GetVaultsQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var vaultDto = result.First();
        Assert.Equal("Test Vault", vaultDto.Name);
        Assert.Equal(2, vaultDto.ItemCount); // Should only count non-deleted items (item1 and item2)
    }

    [Fact]
    public async Task Handle_Should_LogInformation_WhenReturningFromCache_AndLogLevelEnabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cachedVaults = new List<VaultDto>
        {
            new VaultDto
            {
                Id = Guid.NewGuid(),
                Name = "Cached Vault",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedVaults);

        mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(true);

        var request = new GetVaultsQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returning vaults from cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_NotLogInformation_WhenReturningFromCache_AndLogLevelDisabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cachedVaults = new List<VaultDto>
        {
            new VaultDto
            {
                Id = Guid.NewGuid(),
                Name = "Cached Vault",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedVaults);

        mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(false);

        var request = new GetVaultsQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.IsEnabled(LogLevel.Information),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returning vaults from cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_LogInformation_WhenGettingFromDatabase_AndLogLevelEnabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        mockCacheService
            .Setup(x => x.GetAsync<List<VaultDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<VaultDto>?)null);

        var repoVaults = new List<Domain.Entities.Vault>
        {
            Domain.Entities.Vault.Create(
                userId,
                "Repo Vault",
                Domain.ValueObjects.EncryptedData.Create("ciphertext", "iv"))
        };

        mockVaultRepository
            .Setup(x => x.GetByOwnerIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoVaults);

        mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(true);

        var request = new GetVaultsQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting vaults from database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_NotLogInformation_WhenGettingFromDatabase_AndLogLevelDisabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        mockCacheService
            .Setup(x => x.GetAsync<List<VaultDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<VaultDto>?)null);

        var repoVaults = new List<Domain.Entities.Vault>
        {
            Domain.Entities.Vault.Create(
                userId,
                "Repo Vault",
                Domain.ValueObjects.EncryptedData.Create("ciphertext", "iv"))
        };

        mockVaultRepository
            .Setup(x => x.GetByOwnerIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repoVaults);

        mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(false);

        var request = new GetVaultsQuery();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.IsEnabled(LogLevel.Information),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting vaults from database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
