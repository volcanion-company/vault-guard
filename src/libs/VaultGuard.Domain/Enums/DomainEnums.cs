namespace VaultGuard.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Suspended = 2,
    Deleted = 3
}

public enum VaultItemType
{
    Password = 1,
    SecureNote = 2,
    CreditCard = 3,
    Identity = 4,
    Document = 5
}

public enum AuditAction
{
    VaultCreated = 1,
    VaultUpdated = 2,
    VaultDeleted = 3,
    VaultItemCreated = 4,
    VaultItemUpdated = 5,
    VaultItemDeleted = 6,
    VaultItemViewed = 7,
    VaultShared = 8,
    LoginSuccess = 9,
    LoginFailed = 10,
    PasswordChanged = 11,
    DeviceRegistered = 12
}
