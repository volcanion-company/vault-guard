namespace VaultGuard.Application.Tests.Features.VaultItems.Command;

public class CreateVaultItemCommandValidatorTests
{
    [Fact]
    public void CreateVaultItemCommandValidator_Should_Be_Initializable()
    {
        // Arrange & Act
        var validator = new VaultGuard.Application.Features.VaultItems.Commands.CreateVaultItemCommandValidator();
        // Assert
        Assert.NotNull(validator);
    }

    [Fact]
    public void CreateVaultItemCommandValidator_Should_Validate_Command_Correctly()
    {
        // Arrange
        var validator = new VaultGuard.Application.Features.VaultItems.Commands.CreateVaultItemCommandValidator();
        var command = new VaultGuard.Application.Features.VaultItems.Commands.CreateVaultItemCommand
        {
            VaultId = Guid.NewGuid(),
            EncryptedPayloadCipherText = "ciphertext",
            EncryptedPayloadIV = "iv",
            Metadata = "metadata",
            Type = Domain.Enums.VaultItemType.Password
        };
        // Act
        var result = validator.Validate(command);
        // Assert
        Assert.True(result.IsValid);
    }
}
