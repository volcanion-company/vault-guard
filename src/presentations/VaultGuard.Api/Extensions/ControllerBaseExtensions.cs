using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VaultGuard.API.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ControllerBase"/> to simplify access to user claims and request metadata
/// in ASP.NET Core controllers.
/// </summary>
/// <remarks>These methods enable convenient retrieval of common user information, such as user ID, email, and
/// roles, from the current HTTP context. They also provide ways to check user roles and obtain the request identifier
/// associated with the current request. All methods assume that the controller's <see cref="ControllerBase.User"/>
/// property is populated with claims from a valid authentication scheme. Exceptions may be thrown if required claims
/// are missing; see individual method documentation for details.</remarks>
public static class ControllerBaseExtensions
{
    /// <summary>
    /// Retrieves the unique identifier of the current authenticated user from the controller's claims.
    /// </summary>
    /// <remarks>This method requires that the user is authenticated and that the NameIdentifier claim is
    /// present and contains a valid GUID. It is typically used in controller actions to identify the current
    /// user.</remarks>
    /// <param name="controller">The controller instance from which to extract the current user's identifier. Must have an authenticated user
    /// with a valid NameIdentifier claim.</param>
    /// <returns>A <see cref="Guid"/> representing the current user's unique identifier.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user is not authenticated or the NameIdentifier claim is missing or invalid.</exception>
    public static Guid GetCurrentUserId(this ControllerBase controller)
    {
        // Extract the NameIdentifier claim
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            // Invalid or missing user ID claim
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        // Return the parsed user ID
        return userId;
    }

    /// <summary>
    /// Attempts to retrieve the current user's unique identifier from the specified controller's user claims.
    /// </summary>
    /// <remarks>This method looks for a claim of type <see cref="ClaimTypes.NameIdentifier"/> in the
    /// controller's user principal. If the claim is missing or cannot be parsed as a <see cref="Guid"/>, the method
    /// returns <see langword="null"/>.</remarks>
    /// <param name="controller">The controller instance from which to extract the current user's identifier. Must not be null and should have an
    /// authenticated user with a NameIdentifier claim.</param>
    /// <returns>A <see cref="Guid"/> representing the user's unique identifier if available and valid; otherwise, <see
    /// langword="null"/>.</returns>
    public static Guid? TryGetCurrentUserId(this ControllerBase controller)
    {
        // Extract the NameIdentifier claim
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Try to parse and return the user ID, or null if invalid
        return userIdClaim != null && Guid.TryParse(userIdClaim, out var userId) ? userId  : null;
    }

    /// <summary>
    /// Retrieves the email address of the currently authenticated user from the specified controller's claims.
    /// </summary>
    /// <remarks>This method relies on the presence of an email claim in the user's identity. Ensure that
    /// authentication middleware populates the email claim for users. Typically used in controllers to access the
    /// authenticated user's email for authorization or personalization purposes.</remarks>
    /// <param name="controller">The controller instance from which to obtain the current user's email address. Must have an authenticated user
    /// with an email claim.</param>
    /// <returns>The email address of the current user as a string.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the current user's claims do not contain an email address.</exception>
    public static string GetCurrentUserEmail(this ControllerBase controller)
    {
        // Extract the email claim
        return controller.User.FindFirst(ClaimTypes.Email)?.Value ?? throw new UnauthorizedAccessException("Email not found in claims");
    }

    /// <summary>
    /// Retrieves the email address of the current authenticated user from the specified controller, if available.
    /// </summary>
    /// <remarks>This method extracts the email claim from the user associated with the provided controller.
    /// If the user is not authenticated or does not have an email claim, the method returns null.</remarks>
    /// <param name="controller">The controller instance from which to obtain the current user's email address. Must not be null.</param>
    /// <returns>The email address of the current user if present; otherwise, null.</returns>
    public static string? TryGetCurrentUserEmail(this ControllerBase controller)
    {
        // Extract and return the email claim value, or null if not present
        return controller.User.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Retrieves the list of role names associated with the current user of the specified controller.
    /// </summary>
    /// <param name="controller">The controller instance whose current user's roles are to be retrieved. Must not be null.</param>
    /// <returns>A list of strings containing the names of all roles assigned to the current user. The list will be empty if the
    /// user has no roles.</returns>
    public static List<string> GetCurrentUserRoles(this ControllerBase controller)
    {
        // Extract and return all role claims as a list of strings
        return [.. controller.User.FindAll(ClaimTypes.Role).Select(c => c.Value)];
    }

    /// <summary>
    /// Determines whether the current user associated with the specified controller is a member of the given role.
    /// </summary>
    /// <remarks>This method is an extension for ControllerBase and relies on the controller's User property.
    /// Ensure that authentication is configured and the User property is populated before calling this
    /// method.</remarks>
    /// <param name="controller">The controller instance whose current user is evaluated for role membership. Cannot be null.</param>
    /// <param name="role">The name of the role to check for membership. Cannot be null or empty.</param>
    /// <returns>true if the current user is in the specified role; otherwise, false.</returns>
    public static bool CurrentUserHasRole(this ControllerBase controller, string role)
    {
        // Check if the user is in the specified role
        return controller.User.IsInRole(role);
    }

    /// <summary>
    /// Retrieves the request identifier associated with the current HTTP context for the specified controller.
    /// </summary>
    /// <remarks>The request identifier is expected to be stored in the HTTP context's Items collection under
    /// the key "RequestId". This method returns null if the identifier is not set or if the key is missing.</remarks>
    /// <param name="controller">The controller instance from which to obtain the request identifier. Must not be null.</param>
    /// <returns>A string containing the request identifier if present; otherwise, null.</returns>
    public static string? GetRequestId(this ControllerBase controller)
    {
        // Retrieve and return the RequestId from HttpContext.Items
        return controller.HttpContext.Items["RequestId"]?.ToString();
    }
}
