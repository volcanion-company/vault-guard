namespace VaultGuard.Application.Tests.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void ValidationException_Should_Be_Initializable_Without_Errors()
    {
        // Arrange & Act
        var exception = new VaultGuard.Application.Exceptions.ValidationException();
        // Assert
        Assert.NotNull(exception);
        Assert.NotNull(exception.Errors);
        Assert.Empty(exception.Errors);
        Assert.Equal("One or more validation failures have occurred.", exception.Message);
    }

    [Fact]
    public void ValidationException_Should_Be_Initializable_With_Errors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error1", "Error2" } },
            { "Field2", new[] { "Error3" } }
        };
        // Act
        var exception = new VaultGuard.Application.Exceptions.ValidationException(errors);
        // Assert
        Assert.NotNull(exception);
        Assert.Equal(errors, exception.Errors);
        Assert.Equal("One or more validation failures have occurred.", exception.Message);
    }
}
