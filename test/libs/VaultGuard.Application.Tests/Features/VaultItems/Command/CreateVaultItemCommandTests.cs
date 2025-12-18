namespace VaultGuard.Application.Tests.Features.VaultItems.Command;

public class CreateVaultItemCommandTests
{
    [Fact]
    public void CreateVaultItemCommand_Should_Be_Initializable()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var username = "testuser";
        var password = "P@ssw0rd!";
        var notes = "These are some test notes.";
        // Act
        var command = new VaultGuard.Application.Features.VaultItems.Commands.CreateVaultItemCommand
        {
            VaultId = vaultId,
            EncryptedPayloadCipherText = password,
            EncryptedPayloadIV = username,
            Metadata = notes,
            Type = Domain.Enums.VaultItemType.Document
        };
        // Assert
        Assert.NotNull(command);
        Assert.Equal(vaultId, command.VaultId);
        Assert.Equal(password, command.EncryptedPayloadCipherText);
        Assert.Equal(username, command.EncryptedPayloadIV);
        Assert.Equal(notes, command.Metadata);
        Assert.Equal(Domain.Enums.VaultItemType.Document, command.Type);
    }
}
