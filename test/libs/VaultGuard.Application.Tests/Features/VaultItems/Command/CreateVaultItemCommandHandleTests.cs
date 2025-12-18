using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using VaultGuard.Application.Common.Interfaces;
using VaultGuard.Application.Features.VaultItems.Commands;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Repositories;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Application.Tests.Features.VaultItems.Command;

public class CreateVaultItemCommandHandleTests
{
    private readonly Mock<IVaultRepository> mockVaultRepository;
    private readonly Mock<IAuditLogRepository> mockAuditLogRepository;
    private readonly Mock<IUnitOfWork> mockUnitOfWork;
    private readonly Mock<ICurrentUserService> mockCurrentUserService;
    private readonly Mock<ICacheService> mockCacheService;
    private readonly Mock<ILogger<CreateVaultItemCommandHandler>> mockLogger;
    private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
    private readonly CreateVaultItemCommandHandler handler;

    public CreateVaultItemCommandHandleTests()
    {
        mockVaultRepository = new Mock<IVaultRepository>();
        mockAuditLogRepository = new Mock<IAuditLogRepository>();
        mockUnitOfWork = new Mock<IUnitOfWork>();
        mockCurrentUserService = new Mock<ICurrentUserService>();
        mockCacheService = new Mock<ICacheService>();
        mockLogger = new Mock<ILogger<CreateVaultItemCommandHandler>>();
        mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        handler = new CreateVaultItemCommandHandler(
            mockVaultRepository.Object,
            mockAuditLogRepository.Object,
            mockUnitOfWork.Object,
            mockCurrentUserService.Object,
            mockCacheService.Object,
            mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_Should_Throw_UnauthorizedAccessException()
    {
        // Arrange
        var request = new CreateVaultItemCommand
        {
            VaultId = Guid.NewGuid(),
            EncryptedPayloadCipherText = "ciphertext",
            EncryptedPayloadIV = "iv",
            Metadata = "metadata",
            Type = Domain.Enums.VaultItemType.Password
        };

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vault?)null);

        // Act & Assert
        var result = handler.Handle(request, CancellationToken.None);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await result);
    }

    [Fact]
    public async Task Handle_Should_ReturnData()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var request = new CreateVaultItemCommand
        {
            VaultId = vaultId,
            EncryptedPayloadCipherText = "ciphertext",
            EncryptedPayloadIV = "iv",
            Metadata = "metadata",
            Type = Domain.Enums.VaultItemType.Password
        };
        
        var encryptedPayload = EncryptedData.Create(request.EncryptedPayloadCipherText, request.EncryptedPayloadIV);
        var vault = Vault.Create(vaultId, "Test Vault", encryptedPayload);

        // Mock http context to have the current user id as the vault owner id
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(new DefaultHttpContext());
        mockCurrentUserService.Setup(x => x.UserId).Returns(vault.OwnerId);

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vault);


        // Act & Assert
        var result = handler.Handle(request, CancellationToken.None);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_Should_LogInformation_WhenLogLevelEnabled()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateVaultItemCommand
        {
            VaultId = vaultId,
            EncryptedPayloadCipherText = "ciphertext",
            EncryptedPayloadIV = "iv",
            Metadata = "metadata",
            Type = Domain.Enums.VaultItemType.Password
        };

        var encryptedPayload = EncryptedData.Create(request.EncryptedPayloadCipherText, request.EncryptedPayloadIV);
        var vault = Vault.Create(userId, "Test Vault", encryptedPayload);

        mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        mockCurrentUserService.Setup(x => x.IpAddress).Returns("127.0.0.1");
        mockCurrentUserService.Setup(x => x.UserAgent).Returns("Test Agent");

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(vaultId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vault);

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating vault item in vault {vaultId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("created successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_NotLogInformation_WhenLogLevelDisabled()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateVaultItemCommand
        {
            VaultId = vaultId,
            EncryptedPayloadCipherText = "ciphertext",
            EncryptedPayloadIV = "iv",
            Metadata = "metadata",
            Type = Domain.Enums.VaultItemType.Password
        };

        var encryptedPayload = EncryptedData.Create(request.EncryptedPayloadCipherText, request.EncryptedPayloadIV);
        var vault = Vault.Create(userId, "Test Vault", encryptedPayload);

        mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        mockCurrentUserService.Setup(x => x.IpAddress).Returns("127.0.0.1");
        mockCurrentUserService.Setup(x => x.UserAgent).Returns("Test Agent");

        mockVaultRepository
            .Setup(x => x.GetByIdAsync(vaultId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vault);

        mockLogger
            .Setup(x => x.IsEnabled(LogLevel.Information))
            .Returns(false);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.IsEnabled(LogLevel.Information),
            Times.Once);

        // Verify the conditional log is NOT called when IsEnabled returns false
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating vault item in vault {vaultId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
