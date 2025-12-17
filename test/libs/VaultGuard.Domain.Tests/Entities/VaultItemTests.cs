using FluentAssertions;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Domain.Tests.Entities;

public sealed class VaultItemTests
{
    [Fact]
    public void Create_ShouldCreateVaultItem_WhenValidParametersProvided()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var type = VaultItemType.Password;
        var encryptedPayload = EncryptedData.Create("cipher", "iv");
        var metadata = "Login for website";

        // Act
        var item = VaultItem.Create(vaultId, type, encryptedPayload, metadata);

        // Assert
        item.Should().NotBeNull();
        item.Id.Should().NotBeEmpty();
        item.VaultId.Should().Be(vaultId);
        item.Type.Should().Be(type);
        item.EncryptedPayload.Should().Be(encryptedPayload);
        item.Metadata.Should().Be(metadata);
        item.IsDeleted.Should().BeFalse();
        item.LastAccessedAt.Should().BeNull();
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldCreateVaultItem_WhenMetadataIsNull()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var type = VaultItemType.SecureNote;
        var encryptedPayload = EncryptedData.Create("cipher", "iv");

        // Act
        var item = VaultItem.Create(vaultId, type, encryptedPayload);

        // Assert
        item.Should().NotBeNull();
        item.Metadata.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenVaultIdIsEmpty()
    {
        // Arrange
        var vaultId = Guid.Empty;
        var type = VaultItemType.Password;
        var encryptedPayload = EncryptedData.Create("cipher", "iv");

        // Act
        var act = () => VaultItem.Create(vaultId, type, encryptedPayload);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*VaultId*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenEncryptedPayloadIsNull()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var type = VaultItemType.Password;

        // Act
        var act = () => VaultItem.Create(vaultId, type, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Update_ShouldUpdateEncryptedPayload_WhenValidPayloadProvided()
    {
        // Arrange
        var item = CreateDefaultVaultItem();
        var newPayload = EncryptedData.Create("new_cipher", "new_iv");

        // Act
        item.Update(newPayload);

        // Assert
        item.EncryptedPayload.Should().Be(newPayload);
        item.UpdatedAt.Should().NotBeNull();
        item.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_ShouldUpdateMetadata_WhenValidMetadataProvided()
    {
        // Arrange
        var item = CreateDefaultVaultItem();
        var newPayload = EncryptedData.Create("new_cipher", "new_iv");
        var newMetadata = "Updated metadata";

        // Act
        item.Update(newPayload, newMetadata);

        // Assert
        item.EncryptedPayload.Should().Be(newPayload);
        item.Metadata.Should().Be(newMetadata);
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldThrowArgumentNullException_WhenEncryptedPayloadIsNull()
    {
        // Arrange
        var item = CreateDefaultVaultItem();

        // Act
        var act = () => item.Update(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Delete_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var item = CreateDefaultVaultItem();

        // Act
        item.Delete();

        // Assert
        item.IsDeleted.Should().BeTrue();
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsAccessed_ShouldUpdateLastAccessedAt()
    {
        // Arrange
        var item = CreateDefaultVaultItem();
        var initialLastAccessed = item.LastAccessedAt;

        // Act
        item.MarkAsAccessed();

        // Assert
        item.LastAccessedAt.Should().NotBeNull();
        item.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        item.LastAccessedAt.Should().NotBe(initialLastAccessed);
    }

    [Fact]
    public void MarkAsAccessed_ShouldUpdateOnMultipleCalls()
    {
        // Arrange
        var item = CreateDefaultVaultItem();
        
        // Act
        item.MarkAsAccessed();
        var firstAccess = item.LastAccessedAt;
        
        Thread.Sleep(10); // Small delay to ensure different timestamp
        item.MarkAsAccessed();
        var secondAccess = item.LastAccessedAt;

        // Assert
        secondAccess.Should().BeAfter(firstAccess!.Value);
    }

    [Theory]
    [InlineData(VaultItemType.Password)]
    [InlineData(VaultItemType.SecureNote)]
    [InlineData(VaultItemType.CreditCard)]
    [InlineData(VaultItemType.Identity)]
    [InlineData(VaultItemType.Document)]
    public void Create_ShouldSupportAllVaultItemTypes(VaultItemType type)
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var encryptedPayload = EncryptedData.Create("cipher", "iv");

        // Act
        var item = VaultItem.Create(vaultId, type, encryptedPayload);

        // Assert
        item.Type.Should().Be(type);
    }

    private static VaultItem CreateDefaultVaultItem()
    {
        return VaultItem.Create(
            Guid.NewGuid(),
            VaultItemType.Password,
            EncryptedData.Create("cipher", "iv"),
            "Test metadata");
    }
}
