using System.Security.Claims;

namespace VaultGuard.API.Extensions;

/// <summary>
/// Provides extension methods for <see cref="HttpContext"/> to simplify access to user information, request metadata,
/// and custom context data within an ASP.NET Core application.
/// </summary>
/// <remarks>These methods enable convenient retrieval of common user claims (such as user ID, email, and roles),
/// management of per-request identifiers, and storage or retrieval of custom user-specific context data. All methods
/// assume that the <see cref="HttpContext"/> instance is valid and that user claims are populated by the authentication
/// middleware. Thread safety is ensured by the ASP.NET Core request model, as each <see cref="HttpContext"/> instance
/// is scoped to a single request.</remarks>
public static class HttpContextExtensions
{
    /// <summary>
    /// Retrieves the user identifier (ID) from the current HTTP context, if available.
    /// </summary>
    /// <remarks>The user ID is obtained from the <see cref="ClaimTypes.NameIdentifier"/> claim of the
    /// authenticated user. If the claim is missing or cannot be parsed as a <see cref="Guid"/>, the method returns <see
    /// langword="null"/>.</remarks>
    /// <param name="context">The HTTP context containing the user principal from which to extract the user ID. Cannot be null.</param>
    /// <returns>A <see cref="Guid"/> representing the user's identifier if present and valid; otherwise, <see langword="null"/>.</returns>
    public static Guid? GetUserId(this HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null && Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Retrieves the email address associated with the current authenticated user in the specified HTTP context.
    /// </summary>
    /// <remarks>This method returns the value of the email claim from the user's identity. If the user is not
    /// authenticated or the email claim is not present, the method returns null.</remarks>
    /// <param name="context">The HTTP context containing the user whose email address is to be retrieved. Must not be null.</param>
    /// <returns>The email address of the authenticated user if available; otherwise, null.</returns>
    public static string? GetUserEmail(this HttpContext context)
    {
        // Extract and return the email claim value, or null if not present
        return context.User.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Retrieves the list of role names associated with the current user in the specified HTTP context.
    /// </summary>
    /// <remarks>This method extracts roles from the claims of the user principal in the provided HTTP
    /// context. It is typically used in ASP.NET Core applications to determine user authorization based on
    /// roles.</remarks>
    /// <param name="context">The HTTP context containing the user whose roles are to be retrieved. Must not be null.</param>
    /// <returns>A list of strings representing the role names assigned to the current user. The list will be empty if the user
    /// has no roles.</returns>
    public static List<string> GetUserRoles(this HttpContext context)
    {
        // Extract and return all role claims as a list of strings
        return context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    /// <summary>
    /// Determines whether the current user associated with the specified HTTP context is in the given role.
    /// </summary>
    /// <remarks>This method relies on the authentication and role management configured for the application.
    /// If the user is not authenticated, the method returns false.</remarks>
    /// <param name="context">The HTTP context containing the user whose roles are to be checked. Cannot be null.</param>
    /// <param name="role">The name of the role to check for membership. Cannot be null or empty.</param>
    /// <returns>true if the current user is a member of the specified role; otherwise, false.</returns>
    public static bool HasRole(this HttpContext context, string role)
    {
        // Check if the user is in the specified role
        return context.User.IsInRole(role);
    }

    /// <summary>
    /// Retrieves the request identifier associated with the specified HTTP context, if available.
    /// </summary>
    /// <remarks>The request identifier is expected to be stored in the <see cref="HttpContext.Items"/>
    /// collection under the key "RequestId". If no identifier has been set for the current request, this method returns
    /// <see langword="null"/>.</remarks>
    /// <param name="context">The <see cref="HttpContext"/> instance from which to obtain the request identifier. Cannot be null.</param>
    /// <returns>A string containing the request identifier if present; otherwise, <see langword="null"/>.</returns>
    public static string? GetRequestId(this HttpContext context)
    {
        // Retrieve and return the RequestId from HttpContext.Items
        return context.Items["RequestId"]?.ToString();
    }

    /// <summary>
    /// Associates a user-specific value with the current HTTP context using the specified key.
    /// </summary>
    /// <remarks>This method stores the value in the context's Items collection under a namespaced key. The
    /// value will be available for the duration of the current HTTP request and can be retrieved using the same
    /// key.</remarks>
    /// <param name="context">The HTTP context to which the user-specific value will be attached. Cannot be null.</param>
    /// <param name="key">The key used to identify the user-specific value within the context. Cannot be null or empty.</param>
    /// <param name="value">The value to associate with the specified key. Can be any object, including null.</param>
    public static void SetUserContext(this HttpContext context, string key, object value)
    {
        // Store the value in the Items collection using a namespaced key
        context.Items[$"UserContext:{key}"] = value;
    }

    /// <summary>
    /// Retrieves a user-specific context value of the specified type from the current HTTP context using the provided
    /// key.
    /// </summary>
    /// <remarks>This method looks for a value stored in the Items collection of the specified HttpContext,
    /// using a key prefixed with "UserContext:". If the value exists and is of the requested type, it is returned;
    /// otherwise, the default value for type T is returned. This is useful for accessing per-request user data in
    /// middleware or controllers.</remarks>
    /// <typeparam name="T">The type of the context value to retrieve.</typeparam>
    /// <param name="context">The HTTP context from which to retrieve the user context value. Cannot be null.</param>
    /// <param name="key">The key identifying the user context value to retrieve. Cannot be null or empty.</param>
    /// <returns>The context value of type T associated with the specified key, or the default value for type T if the key is not
    /// found or the value is not of type T.</returns>
    public static T? GetUserContext<T>(this HttpContext context, string key)
    {
        // Try to get the value from the Items collection using the specified key
        if (context.Items.TryGetValue($"UserContext:{key}", out var value) && value is T typedValue)
        {
            // Return the value if it exists and is of the correct type
            return typedValue;
        }
        // Return the default value for type T if the key is not found or the value is of a different type
        return default;
    }
}
