using VaultGuard.Domain.Common;
using VaultGuard.Domain.Enums;
using VaultGuard.Domain.ValueObjects;

namespace VaultGuard.Domain.Entities;

/// <summary>
/// User aggregate root - represents a user in the system
/// </summary>
public sealed class User : BaseEntity, IAggregateRoot
{
    public string Email { get; private set; }
    public string PasswordVerifier { get; private set; }
    public EncryptedData EncryptedMasterKey { get; private set; }
    public UserStatus Status { get; private set; }

    // Navigation properties
    private readonly List<Vault> _vaults = new();
    public IReadOnlyCollection<Vault> Vaults => _vaults.AsReadOnly();

    private readonly List<Device> _devices = new();
    public IReadOnlyCollection<Device> Devices => _devices.AsReadOnly();

    private User() { } // EF Core

    private User(string email, string passwordVerifier, EncryptedData encryptedMasterKey) : base()
    {
        Email = email;
        PasswordVerifier = passwordVerifier;
        EncryptedMasterKey = encryptedMasterKey;
        Status = UserStatus.Active;
    }

    public static User Create(string email, string passwordVerifier, EncryptedData encryptedMasterKey)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordVerifier))
            throw new ArgumentException("PasswordVerifier cannot be empty", nameof(passwordVerifier));

        if (encryptedMasterKey == null)
            throw new ArgumentNullException(nameof(encryptedMasterKey));

        return new User(email, passwordVerifier, encryptedMasterKey);
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
        MarkAsUpdated();
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        MarkAsUpdated();
    }

    public void Delete()
    {
        Status = UserStatus.Deleted;
        MarkAsUpdated();
    }

    public void UpdatePasswordVerifier(string newPasswordVerifier, EncryptedData newEncryptedMasterKey)
    {
        if (string.IsNullOrWhiteSpace(newPasswordVerifier))
            throw new ArgumentException("PasswordVerifier cannot be empty", nameof(newPasswordVerifier));

        if (newEncryptedMasterKey == null)
            throw new ArgumentNullException(nameof(newEncryptedMasterKey));

        PasswordVerifier = newPasswordVerifier;
        EncryptedMasterKey = newEncryptedMasterKey;
        MarkAsUpdated();
    }

    public void AddDevice(Device device)
    {
        if (device == null)
            throw new ArgumentNullException(nameof(device));

        _devices.Add(device);
        MarkAsUpdated();
    }

    public void AddVault(Vault vault)
    {
        if (vault == null)
            throw new ArgumentNullException(nameof(vault));

        _vaults.Add(vault);
        MarkAsUpdated();
    }
}
