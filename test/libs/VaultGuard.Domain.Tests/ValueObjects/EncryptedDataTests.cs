using FluentAssertions;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Domain.Tests.ValueObjects;

public sealed class EncryptedDataTests
{
    [Fact]
    public void Create_ShouldCreateEncryptedData_WhenValidParametersProvided()
    {
        // Arrange
        var cipherText = "encrypted_cipher_text";
        var iv = "initialization_vector";

        // Act
        var encryptedData = EncryptedData.Create(cipherText, iv);

        // Assert
        encryptedData.Should().NotBeNull();
        encryptedData.CipherText.Should().Be(cipherText);
        encryptedData.InitializationVector.Should().Be(iv);
    }

    [Theory]
    [InlineData(null, "iv")]
    [InlineData("", "iv")]
    [InlineData("   ", "iv")]
    public void Create_ShouldThrowArgumentException_WhenCipherTextIsNullOrEmpty(string? cipherText, string iv)
    {
        // Act
        var act = () => EncryptedData.Create(cipherText!, iv);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*CipherText*");
    }

    [Theory]
    [InlineData("cipher", null)]
    [InlineData("cipher", "")]
    [InlineData("cipher", "   ")]
    public void Create_ShouldThrowArgumentException_WhenIVIsNullOrEmpty(string cipherText, string? iv)
    {
        // Act
        var act = () => EncryptedData.Create(cipherText, iv!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*InitializationVector*");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenTwoInstancesHaveSameValues()
    {
        // Arrange
        var cipherText = "cipher";
        var iv = "iv";
        var data1 = EncryptedData.Create(cipherText, iv);
        var data2 = EncryptedData.Create(cipherText, iv);

        // Act & Assert
        data1.Equals(data2).Should().BeTrue();
        data1.GetHashCode().Should().Be(data2.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCipherTextDiffers()
    {
        // Arrange
        var data1 = EncryptedData.Create("cipher1", "iv");
        var data2 = EncryptedData.Create("cipher2", "iv");

        // Act & Assert
        data1.Equals(data2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenIVDiffers()
    {
        // Arrange
        var data1 = EncryptedData.Create("cipher", "iv1");
        var data2 = EncryptedData.Create("cipher", "iv2");

        // Act & Assert
        data1.Equals(data2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedWithNull()
    {
        // Arrange
        var data = EncryptedData.Create("cipher", "iv");

        // Act & Assert
        data.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedWithDifferentType()
    {
        // Arrange
        var data = EncryptedData.Create("cipher", "iv");

        // Act & Assert
        data.Equals("string").Should().BeFalse();
    }
}
