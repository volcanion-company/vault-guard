using FluentAssertions;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Domain.Tests.Entities;

public sealed class VaultTests
{
    [Fact]
    public void Create_ShouldCreateVault_WhenValidParametersProvided()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var name = "My Vault";
        var encryptedKey = EncryptedData.Create("cipher", "iv");

        // Act
        var vault = Vault.Create(ownerId, name, encryptedKey);

        // Assert
        vault.Should().NotBeNull();
        vault.Id.Should().NotBeEmpty();
        vault.OwnerId.Should().Be(ownerId);
        vault.Name.Should().Be(name);
        vault.EncryptedVaultKey.Should().Be(encryptedKey);
        vault.Version.Should().Be(1);
        vault.IsDeleted.Should().BeFalse();
        vault.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenOwnerIdIsEmpty()
    {
        // Arrange
        var ownerId = Guid.Empty;
        var name = "My Vault";
        var encryptedKey = EncryptedData.Create("cipher", "iv");

        // Act
        var act = () => Vault.Create(ownerId, name, encryptedKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*OwnerId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenNameIsNullOrEmpty(string? name)
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var encryptedKey = EncryptedData.Create("cipher", "iv");

        // Act
        var act = () => Vault.Create(ownerId, name!, encryptedKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenEncryptedKeyIsNull()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var name = "My Vault";

        // Act
        var act = () => Vault.Create(ownerId, name, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateName_ShouldUpdateName_WhenValidNameProvided()
    {
        // Arrange
        var vault = CreateDefaultVault();
        var newName = "Updated Vault";
        var originalVersion = vault.Version;

        // Act
        vault.UpdateName(newName);

        // Assert
        vault.Name.Should().Be(newName);
        vault.Version.Should().Be(originalVersion + 1);
        vault.UpdatedAt.Should().NotBeNull();
        vault.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_ShouldThrowArgumentException_WhenNameIsNullOrEmpty(string? name)
    {
        // Arrange
        var vault = CreateDefaultVault();

        // Act
        var act = () => vault.UpdateName(name!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name*");
    }

    [Fact]
    public void Delete_ShouldMarkVaultAsDeleted()
    {
        // Arrange
        var vault = CreateDefaultVault();

        // Act
        vault.Delete();

        // Assert
        vault.IsDeleted.Should().BeTrue();
        vault.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddItem_ShouldAddItemToVault_WhenValidParametersProvided()
    {
        // Arrange
        var vault = CreateDefaultVault();
        var type = VaultItemType.Password;
        var encryptedPayload = EncryptedData.Create("payload", "iv");
        var metadata = "Login credentials";
        var originalVersion = vault.Version;

        // Act
        var item = vault.AddItem(type, encryptedPayload, metadata);

        // Assert
        item.Should().NotBeNull();
        item.VaultId.Should().Be(vault.Id);
        item.Type.Should().Be(type);
        item.EncryptedPayload.Should().Be(encryptedPayload);
        item.Metadata.Should().Be(metadata);
        vault.Items.Should().Contain(item);
        vault.Version.Should().Be(originalVersion + 1);
    }

    [Fact]
    public void AddItem_ShouldIncrementVersion()
    {
        // Arrange
        var vault = CreateDefaultVault();
        var originalVersion = vault.Version;

        // Act
        vault.AddItem(VaultItemType.Password, EncryptedData.Create("payload", "iv"));
        vault.AddItem(VaultItemType.SecureNote, EncryptedData.Create("note", "iv2"));

        // Assert
        vault.Version.Should().Be(originalVersion + 2);
    }

    [Fact]
    public void RemoveItem_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var vault = CreateDefaultVault();
        var item = vault.AddItem(VaultItemType.Password, EncryptedData.Create("payload", "iv"));
        var versionAfterAdd = vault.Version;

        // Act
        vault.RemoveItem(item);

        // Assert
        item.IsDeleted.Should().BeTrue();
        vault.Version.Should().Be(versionAfterAdd + 1);
    }

    [Fact]
    public void RemoveItem_ShouldThrowArgumentNullException_WhenItemIsNull()
    {
        // Arrange
        var vault = CreateDefaultVault();

        // Act
        var act = () => vault.RemoveItem(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureOwnership_ShouldNotThrow_WhenUserIsOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var vault = Vault.Create(ownerId, "Vault", EncryptedData.Create("cipher", "iv"));

        // Act
        var act = () => vault.EnsureOwnership(ownerId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureOwnership_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var vault = Vault.Create(ownerId, "Vault", EncryptedData.Create("cipher", "iv"));

        // Act
        var act = () => vault.EnsureOwnership(otherUserId);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*does not own this vault*");
    }

    [Fact]
    public void Items_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var vault = CreateDefaultVault();

        // Act
        var items = vault.Items;

        // Assert
        items.Should().BeAssignableTo<IReadOnlyCollection<VaultItem>>();
    }

    private static Vault CreateDefaultVault()
    {
        return Vault.Create(
            Guid.NewGuid(),
            "Test Vault",
            EncryptedData.Create("cipher", "iv"));
    }
}
