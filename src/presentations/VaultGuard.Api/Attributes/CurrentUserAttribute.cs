using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace VaultGuard.API.Attributes;

/// <summary>
/// Specifies that the decorated action parameter should be populated with information about the current authenticated
/// user.
/// </summary>
/// <remarks>Apply this attribute to an action method parameter of type <see cref="CurrentUserInfo"/> to
/// automatically bind the current user's identifier, email, and roles from the HTTP context. This attribute is intended
/// for use in ASP.NET Core controllers to simplify access to user details within action methods. The attribute requires
/// that the user is authenticated and that the relevant claims are present in the HTTP context. If the user is not
/// authenticated or required claims are missing, the parameter will not be populated.</remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public class CurrentUserAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Intercepts the execution of an action to inject current user information into the action's parameters when
    /// applicable.
    /// </summary>
    /// <remarks>If the action method defines a parameter of type CurrentUserInfo, this method populates it
    /// with information from the authenticated user's claims before the action executes. This enables action methods to
    /// receive user details directly as a parameter.</remarks>
    /// <param name="context">The context for the executing action, containing HTTP context and action metadata.</param>
    /// <param name="next">A delegate that executes the next action filter or the action itself.</param>
    /// <returns>A task that represents the asynchronous operation of action execution.</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Extract user information from claims
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Extract email and roles
        var email = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = context.HttpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        // Populate CurrentUserInfo if userId is valid
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            // Create CurrentUserInfo instance
            var userInfo = new CurrentUserInfo
            {
                UserId = userGuid,
                Email = email ?? string.Empty,
                Roles = roles
            };

            // Find parameter with CurrentUserAttribute
            var parameter = context.ActionDescriptor.Parameters.FirstOrDefault(p => p.ParameterType == typeof(CurrentUserInfo));
            if (parameter != null)
            {
                // Inject userInfo into action arguments
                context.ActionArguments[parameter.Name] = userInfo;
            }
        }

        // Proceed to the next action filter or action method
        await next();
    }
}

/// <summary>
/// Represents information about the currently authenticated user, including their unique identifier, email address, and
/// assigned roles.
/// </summary>
/// <remarks>This class is typically used to provide user context within an application, such as for authorization
/// checks or personalization. All properties are read-write, allowing updates to user information as needed.</remarks>
public class CurrentUserInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the email address associated with the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; set; } = [];
}
