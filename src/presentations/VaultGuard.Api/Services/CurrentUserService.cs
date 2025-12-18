using VaultGuard.Application.Common.Interfaces;

namespace VaultGuard.Api.Services;

/// <summary>
/// Implementation of ICurrentUserService using HttpContext from JwtAuthenticationMiddleware
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("sub")
                ?? httpContextAccessor.HttpContext?.User?.FindFirst("userId")
                ?? httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated or invalid user ID");
            }

            return userId;
        }
    }

    public string Email
    {
        get
        {
            var emailClaim = httpContextAccessor.HttpContext?.User?.FindFirst("email")
                ?? httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email);

            return emailClaim?.Value ?? throw new UnauthorizedAccessException("User email not available");
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? UserAgent => httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
}
