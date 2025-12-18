using FluentAssertions;
using VaultGuard.Domain.Entities;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Domain.Tests.Entities;

public sealed class UserTests
{
    [Fact]
    public void PrivateConstructor_ShouldBeCallableViaReflection()
    {
        // Arrange
        var constructor = typeof(User).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            Type.EmptyTypes,
            null);

        // Act
        var user = constructor?.Invoke(null) as User;

        // Assert
        user.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldCreateUser_WhenValidParametersProvided()
    {
        // Arrange
        var email = "test@example.com";
        var passwordVerifier = "argon2id_hash_value";
        var encryptedMasterKey = EncryptedData.Create("master_key", "iv2");

        // Act
        var user = User.Create(email, passwordVerifier, encryptedMasterKey);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be(email);
        user.PasswordVerifier.Should().Be(passwordVerifier);
        user.EncryptedMasterKey.Should().Be(encryptedMasterKey);
        user.Status.Should().Be(UserStatus.Active);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenEmailIsNullOrEmpty(string? email)
    {
        // Arrange
        var passwordVerifier = "argon2id_hash";
        var encryptedMasterKey = EncryptedData.Create("master_key", "iv2");

        // Act
        var act = () => User.Create(email!, passwordVerifier, encryptedMasterKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenPasswordVerifierIsNull()
    {
        // Arrange
        var email = "test@example.com";
        var encryptedMasterKey = EncryptedData.Create("master_key", "iv");

        // Act
        var act = () => User.Create(email, null!, encryptedMasterKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*PasswordVerifier*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenEncryptedMasterKeyIsNull()
    {
        // Arrange
        var email = "test@example.com";
        var passwordVerifier = "argon2id_hash";

        // Act
        var act = () => User.Create(email, passwordVerifier, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Suspend_ShouldChangeStatusToSuspended()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        user.Suspend();

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldChangeStatusToActive()
    {
        // Arrange
        var user = CreateDefaultUser();
        user.Suspend();

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Delete_ShouldChangeStatusToDeleted()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        user.Delete();

        // Assert
        user.Status.Should().Be(UserStatus.Deleted);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePasswordVerifier_ShouldUpdatePasswordVerifierAndMasterKey()
    {
        // Arrange
        var user = CreateDefaultUser();
        var newVerifier = "new_argon2id_hash";
        var newMasterKey = EncryptedData.Create("new_master_key", "new_iv");

        // Act
        user.UpdatePasswordVerifier(newVerifier, newMasterKey);

        // Assert
        user.PasswordVerifier.Should().Be(newVerifier);
        user.EncryptedMasterKey.Should().Be(newMasterKey);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePasswordVerifier_ShouldThrowArgumentException_WhenVerifierIsNull()
    {
        // Arrange
        var user = CreateDefaultUser();
        var newMasterKey = EncryptedData.Create("new_master_key", "new_iv");

        // Act
        var act = () => user.UpdatePasswordVerifier(null!, newMasterKey);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*PasswordVerifier*");
    }

    [Fact]
    public void UpdatePasswordVerifier_ShouldThrowArgumentNullException_WhenMasterKeyIsNull()
    {
        // Arrange
        var user = CreateDefaultUser();
        var newVerifier = "new_argon2id_hash";

        // Act
        var act = () => user.UpdatePasswordVerifier(newVerifier, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddDevice_ShouldAddDeviceToUser()
    {
        // Arrange
        var user = CreateDefaultUser();
        var device = Device.Create(user.Id, "iPhone 15 - Mobile App");

        // Act
        user.AddDevice(device);

        // Assert
        user.Devices.Should().Contain(device);
    }

    [Fact]
    public void AddDevice_ShouldThrowArgumentNullException_WhenDeviceIsNull()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        var act = () => user.AddDevice(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddVault_ShouldAddVaultToUser()
    {
        // Arrange
        var user = CreateDefaultUser();
        var vault = Vault.Create(user.Id, "Personal Vault", EncryptedData.Create("vault_key", "iv"));

        // Act
        user.AddVault(vault);

        // Assert
        user.Vaults.Should().Contain(vault);
    }

    [Fact]
    public void AddVault_ShouldThrowArgumentNullException_WhenVaultIsNull()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        var act = () => user.AddVault(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Devices_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        var devices = user.Devices;

        // Assert
        devices.Should().BeAssignableTo<IReadOnlyCollection<Device>>();
    }

    [Fact]
    public void Vaults_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        var vaults = user.Vaults;

        // Assert
        vaults.Should().BeAssignableTo<IReadOnlyCollection<Vault>>();
    }

    [Fact]
    public void StatusTransitions_ShouldAllowMultipleChanges()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act & Assert
        user.Status.Should().Be(UserStatus.Active);
        
        user.Suspend();
        user.Status.Should().Be(UserStatus.Suspended);
        
        user.Activate();
        user.Status.Should().Be(UserStatus.Active);
        
        user.Suspend();
        user.Status.Should().Be(UserStatus.Suspended);
    }

    private static User CreateDefaultUser()
    {
        return User.Create(
            "test@example.com",
            "argon2id_hash_value",
            EncryptedData.Create("master_key", "iv2"));
    }
}
