namespace VaultGuard.Application.Tests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void NotFoundException_Should_Be_Initializable_With_Message()
    {
        // Arrange
        var message = "Entity not found.";
        // Act
        var exception = new VaultGuard.Application.Exceptions.NotFoundException(message);
        // Assert
        Assert.NotNull(exception);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void NotFoundException_Should_Be_Initializable_With_Name_And_Key()
    {
        // Arrange
        var name = "User";
        var key = 123;
        var expectedMessage = $"Entity \"{name}\" ({key}) was not found.";
        // Act
        var exception = new VaultGuard.Application.Exceptions.NotFoundException(name, key);
        // Assert
        Assert.NotNull(exception);
        Assert.Equal(expectedMessage, exception.Message);
    }
}
