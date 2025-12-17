namespace VaultGuard.Domain.ValueObjects;

/// <summary>
/// Value object for encrypted data
/// </summary>
public sealed class EncryptedData
{
    public string CipherText { get; private set; }
    public string InitializationVector { get; private set; }

    private EncryptedData(string cipherText, string initializationVector)
    {
        CipherText = cipherText;
        InitializationVector = initializationVector;
    }

    public static EncryptedData Create(string cipherText, string initializationVector)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            throw new ArgumentException("CipherText cannot be empty", nameof(cipherText));

        if (string.IsNullOrWhiteSpace(initializationVector))
            throw new ArgumentException("InitializationVector cannot be empty", nameof(initializationVector));

        return new EncryptedData(cipherText, initializationVector);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EncryptedData other)
            return false;

        return CipherText == other.CipherText && InitializationVector == other.InitializationVector;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CipherText, InitializationVector);
    }
}
