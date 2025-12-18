namespace VaultGuard.Application.Tests.Features.Vaults;

public class CreateVaultCommandValidatorTests
{
    [Fact]
    public void CreateVaultCommandValidator_Should_Be_Initializable()
    {
        // Arrange & Act
        var validator = new VaultGuard.Application.Features.Vaults.Commands.CreateVaultCommandValidator();
        // Assert
        Assert.NotNull(validator);
    }

    [Fact]
    public void CreateVaultCommandValidator_Should_Validate_Command_Correctly()
    {
        // Arrange
        var validator = new VaultGuard.Application.Features.Vaults.Commands.CreateVaultCommandValidator();
        var command = new VaultGuard.Application.Features.Vaults.Commands.CreateVaultCommand
        {
            Name = "My Vault",
            EncryptedVaultKeyCipherText = "ciphertext",
            EncryptedVaultKeyIV = "iv"
        };
        // Act
        var result = validator.Validate(command);
        // Assert
        Assert.True(result.IsValid);
    }
}
