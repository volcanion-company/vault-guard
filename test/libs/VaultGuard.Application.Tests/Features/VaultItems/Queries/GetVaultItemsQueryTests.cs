using VaultGuard.Application.Features.VaultItems.Queries;

namespace VaultGuard.Application.Tests.Features.VaultItems.Queries;

public class GetVaultItemsQueryTests
{
    [Fact]
    public void GetVaultItemsQuery_Should_Have_VaultId_Property()
    {
        // Arrange
        var vaultId = Guid.NewGuid();
        var query = new GetVaultItemsQuery
        {
            VaultId = vaultId
        };
        // Act & Assert
        Assert.Equal(vaultId, query.VaultId);
    }
}
