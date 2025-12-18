using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.DTOs;
using VaultGuard.Application.Features.VaultItems.Queries;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Repositories;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Application.Tests.Features.VaultItems.Queries;

public class GetVaultItemsQueryHandlerTests
{
    private readonly Mock<IVaultRepository> mockVaultRepository = new();
    private readonly Mock<IVaultItemRepository> mockVaultItemRepository = new();
    private readonly Mock<ICurrentUserService> mockCurrentUserService = new();
    private readonly Mock<ICacheService> mockCacheService = new();
    private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
    private readonly Mock<ILogger<GetVaultItemsQueryHandler>> mockLogger = new();
    private readonly GetVaultItemsQueryHandler handler;

    public GetVaultItemsQueryHandlerTests()
    {
        handler = new GetVaultItemsQueryHandler(
            mockVaultRepository.Object,
            mockVaultItemRepository.Object,
            mockCurrentUserService.Object,
            mockCacheService.Object,
            mockLogger.Object
        );
    }

    [Fact]
    public void Constructor_Should_Be_Initializable()
    {
        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handler_Should_Return_Data_From_Cache()
    {
        // Arrange
        var data = new List<VaultItemDto>
        {
            new() { Id = Guid.NewGuid(), VaultId = Guid.NewGuid(), Type = Domain.Enums.VaultItemType.Password, EncryptedPayload = "EncryptedData1", Version = 1, CreatedAt = DateTime.UtcNow },
        };

        var request = new GetVaultItemsQuery
        {
            VaultId = Guid.NewGuid()
        };

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultItemDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(data);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(data.Count, result.Count());
    }

    [Fact]
    public async Task Handler_Should_Throw_UnauthorizedAccessException()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var request = new GetVaultItemsQuery { VaultId = vaultId };

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultItemDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((List<VaultItemDto>?)null);

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((Vault?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await handler.Handle(request, CancellationToken.None);
        });
    }

    [Fact]
    public async Task Handler_Should_Return_Data_Without_Cache()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        IEnumerable<VaultItem> data = new List<VaultItem>
        {
            VaultItem.Create(
                vaultId: vaultId,
                type: Domain.Enums.VaultItemType.Document,
                encryptedPayload: EncryptedData.Create("CipherText2", "IV2"),
                metadata: "Metadata2"
            )
        };

        var request = new GetVaultItemsQuery { VaultId = vaultId };
        var encryptedPayload = EncryptedData.Create("EncryptedPayloadCipherText", "EncryptedPayloadIV");
        var vault = Vault.Create(vaultId, "Test Vault", encryptedPayload);

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultItemDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((List<VaultItemDto>?)null);

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(vault);

        // Mock http context to have the current user id as the vault owner id
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(new DefaultHttpContext());
        mockCurrentUserService.Setup(x => x.UserId).Returns(vault.OwnerId);
        mockVaultItemRepository
            .Setup(x => x.GetByVaultIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(data);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(data.Count(), result.Count());
    }

    [Fact]
    public async Task Handler_Should_LogInformation_WhenReturningFromCache_AndLogLevelEnabled()
    {
        // Arrange
        var data = new List<VaultItemDto>
        {
            new() { Id = Guid.NewGuid(), VaultId = Guid.NewGuid(), Type = Domain.Enums.VaultItemType.Password, EncryptedPayload = "EncryptedData1", Version = 1, CreatedAt = DateTime.UtcNow },
        };

        var request = new GetVaultItemsQuery
        {
            VaultId = Guid.NewGuid()
        };

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultItemDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(data);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(true);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returning vault items from cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handler_Should_NotLogInformation_WhenReturningFromCache_AndLogLevelDisabled()
    {
        // Arrange
        var data = new List<VaultItemDto>
        {
            new() { Id = Guid.NewGuid(), VaultId = Guid.NewGuid(), Type = Domain.Enums.VaultItemType.Password, EncryptedPayload = "EncryptedData1", Version = 1, CreatedAt = DateTime.UtcNow },
        };

        var request = new GetVaultItemsQuery
        {
            VaultId = Guid.NewGuid()
        };

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultItemDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(data);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(false);

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Returning vault items from cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handler_Should_LogInformation_WhenGettingFromDatabase_AndLogLevelEnabled()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        IEnumerable<VaultItem> data = new List<VaultItem>
        {
            VaultItem.Create(
                vaultId: vaultId,
                type: Domain.Enums.VaultItemType.Document,
                encryptedPayload: EncryptedData.Create("CipherText2", "IV2"),
                metadata: "Metadata2"
            )
        };

        var request = new GetVaultItemsQuery { VaultId = vaultId };
        var encryptedPayload = EncryptedData.Create("EncryptedPayloadCipherText", "EncryptedPayloadIV");
        var vault = Vault.Create(vaultId, "Test Vault", encryptedPayload);

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultItemDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((List<VaultItemDto>?)null);

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(vault);

        mockCurrentUserService.Setup(x => x.UserId).Returns(vault.OwnerId);
        mockVaultItemRepository
            .Setup(x => x.GetByVaultIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(data);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(true);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting vault items from database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handler_Should_NotLogInformation_WhenGettingFromDatabase_AndLogLevelDisabled()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        IEnumerable<VaultItem> data = new List<VaultItem>
        {
            VaultItem.Create(
                vaultId: vaultId,
                type: Domain.Enums.VaultItemType.Document,
                encryptedPayload: EncryptedData.Create("CipherText2", "IV2"),
                metadata: "Metadata2"
            )
        };

        var request = new GetVaultItemsQuery { VaultId = vaultId };
        var encryptedPayload = EncryptedData.Create("EncryptedPayloadCipherText", "EncryptedPayloadIV");
        var vault = Vault.Create(vaultId, "Test Vault", encryptedPayload);

        mockCacheService
            .Setup(x => x.GetAsync<List<VaultItemDto>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((List<VaultItemDto>?)null);

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(vault);

        mockCurrentUserService.Setup(x => x.UserId).Returns(vault.OwnerId);
        mockVaultItemRepository
            .Setup(x => x.GetByVaultIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(data);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(false);

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting vault items from database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
