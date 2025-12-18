using VaultGuard.Application.DTOs;
using VaultGuard.Domain.Enums;

namespace VaultGuard.Application.Tests.DTOs;

public class CommonDtosTests
{
    [Fact]
    public void AuditLogDto_Should_Be_Initializable()
    {
        // Arrange & Act
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dto = new AuditLogDto
        {
            Id = id,
            Action = AuditAction.LoginSuccess,
            Metadata = "Test metadata",
            CreatedAt = now,
            IpAddress = "::1"
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id, dto.Id);
        Assert.Equal(AuditAction.LoginSuccess, dto.Action);
        Assert.Equal("Test metadata", dto.Metadata);
        Assert.Equal(now, dto.CreatedAt);
        Assert.Equal("::1", dto.IpAddress);
    }

    [Fact]
    public void AuditLogDto_Should_Be_Sealed()
    {
        // Arrange & Act
        var dtoType = typeof(AuditLogDto);
        // Assert
        Assert.True(dtoType.IsSealed);
    }

    [Fact]
    public void VaultItemDto_Should_Be_Inheritable()
    {
        // Arrange & Act
        var dtoType = typeof(VaultItemDto);
        // Assert
        Assert.False(dtoType.IsSealed);
    }

    [Fact]
    public void VaultItemDto_Should_Be_Initializable()
    {
        // Arrange & Act
        var id = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dto = new VaultItemDto
        {
            Id = id,
            VaultId = vaultId,
            Type = VaultItemType.Password,
            EncryptedPayload = "EncryptedData",
            Metadata = "Test metadata",
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now,
            LastAccessedAt = now
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id, dto.Id);
        Assert.Equal(vaultId, dto.VaultId);
        Assert.Equal(VaultItemType.Password, dto.Type);
        Assert.Equal("EncryptedData", dto.EncryptedPayload);
        Assert.Equal("Test metadata", dto.Metadata);
        Assert.Equal(1, dto.Version);
        Assert.Equal(now, dto.CreatedAt);
        Assert.Equal(now, dto.UpdatedAt);
        Assert.Equal(now, dto.LastAccessedAt);
    }

    [Fact]
    public void VaultItemDetailDto_Should_Be_Initializable()
    {
        // Arrange & Act
        var id = Guid.NewGuid();
        var vaultId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dto = new VaultItemDetailDto
        {
            Id = id,
            VaultId = vaultId,
            Type = VaultItemType.Password,
            EncryptedPayload = "EncryptedData",
            Metadata = "Test metadata",
            Version = 1,
            CreatedAt = now,
            UpdatedAt = now,
            LastAccessedAt = now,
            VaultName = "My Vault"
        };
        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id, dto.Id);
        Assert.Equal(vaultId, dto.VaultId);
        Assert.Equal(VaultItemType.Password, dto.Type);
        Assert.Equal("EncryptedData", dto.EncryptedPayload);
        Assert.Equal("Test metadata", dto.Metadata);
        Assert.Equal(1, dto.Version);
        Assert.Equal(now, dto.CreatedAt);
        Assert.Equal(now, dto.UpdatedAt);
        Assert.Equal(now, dto.LastAccessedAt);
        Assert.Equal("My Vault", dto.VaultName);
    }

    [Fact]
    public void VaultItemDetailDto_Should_Be_Sealed()
    {
        // Arrange & Act
        var dtoType = typeof(VaultItemDetailDto);
        // Assert
        Assert.True(dtoType.IsSealed);
    }
}
