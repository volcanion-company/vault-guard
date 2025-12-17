using VaultGuard.Domain.Common;

namespace VaultGuard.Domain.Entities;

/// <summary>
/// Device entity - represents a device registered to a user
/// </summary>
public sealed class Device : BaseEntity
{
    public Guid UserId { get; private set; }
    public string DeviceInfo { get; private set; }
    public DateTime LastAccessedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    private Device() { } // EF Core

    private Device(Guid userId, string deviceInfo) : base()
    {
        UserId = userId;
        DeviceInfo = deviceInfo;
        LastAccessedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public static Device Create(Guid userId, string deviceInfo)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(deviceInfo))
            throw new ArgumentException("DeviceInfo cannot be empty", nameof(deviceInfo));

        return new Device(userId, deviceInfo);
    }

    public void UpdateLastAccess()
    {
        LastAccessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}
