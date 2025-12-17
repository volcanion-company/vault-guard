namespace VaultGuard.Application.Common.Interfaces;

/// <summary>
/// Service to get current authenticated user context
/// PROVIDED BY AUTH SERVICE - DO NOT IMPLEMENT
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
