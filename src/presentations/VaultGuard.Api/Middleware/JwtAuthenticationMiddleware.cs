using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace VaultGuard.Api.Middleware;

/// <summary>
/// Middleware that authenticates incoming HTTP requests using JSON Web Tokens (JWT) and populates the user context for
/// downstream components.
/// </summary>
/// <remarks>This middleware extracts the JWT from the Authorization header, validates it against application
/// settings, and sets the authenticated user on the HTTP context. If the token is valid, user information such as user
/// ID, email, and roles are made available via HttpContext.Items for downstream access. If the token is expired or
/// invalid, the middleware responds with a 401 Unauthorized status and does not invoke subsequent middleware. This
/// middleware should be registered early in the pipeline to ensure user context is available for authorization and
/// other components.</remarks>
/// <param name="next">The next middleware delegate in the request processing pipeline. Invoked after JWT authentication is performed.</param>
/// <param name="configuration">The application configuration used to retrieve JWT validation settings such as secret key, issuer, and audience.</param>
/// <param name="logger">The logger used to record authentication events and errors during JWT processing.</param>
public class JwtAuthenticationMiddleware(
    RequestDelegate next,
    IConfiguration configuration,
    ILogger<JwtAuthenticationMiddleware> logger)
{
    /// <summary>
    /// Processes the incoming HTTP request by validating a JWT token from the Authorization header and populating the
    /// user context. If a valid token is present, sets the authenticated user and related claims on the current
    /// request.
    /// </summary>
    /// <remarks>If the JWT token is expired, the response status is set to 401 Unauthorized and an error
    /// message is returned. When a valid token is found, user information such as user ID, email, and roles are
    /// extracted and stored in the HttpContext for downstream access. If no token is present or validation fails, the
    /// request proceeds without an authenticated user.</remarks>
    /// <param name="context">The HTTP context for the current request. Contains information about the request, response, and user.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract token from Authorization header
        var token = ExtractTokenFromHeader(context);
        if (!string.IsNullOrEmpty(token))
        {
            // Validate token and populate HttpContext.User
            try
            {
                // Validate the token
                var principal = ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;

                    // Extract and store user context
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                    var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                    // Store in HttpContext.Items for easy access
                    context.Items["UserId"] = userId != null ? Guid.Parse(userId) : (Guid?)null;
                    context.Items["UserEmail"] = email;
                    context.Items["UserRoles"] = roles;

                    logger.LogDebug("JWT validated: UserId={UserId}, Email={Email}, Roles={Roles}", userId, email, string.Join(",", roles));
                }
            }
            catch (SecurityTokenExpiredException)
            {
                logger.LogWarning("JWT token expired");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Token expired" });
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "JWT validation failed");
            }
        }

        await next(context);
    }

    /// <summary>
    /// Extracts the bearer token from the Authorization header of the specified HTTP context, if present.
    /// </summary>
    /// <remarks>This method returns null if the Authorization header is missing or does not use the Bearer
    /// scheme. The returned token is trimmed of leading and trailing whitespace.</remarks>
    /// <param name="context">The HTTP context containing the request from which to extract the bearer token.</param>
    /// <returns>The bearer token as a string if the Authorization header contains a valid Bearer token; otherwise, null.</returns>
    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        // Get the Authorization header
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            // No Authorization header present
            return null;
        }

        // Check if it starts with "Bearer "
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // Extract and return the token part
            return authHeader["Bearer ".Length..].Trim();
        }

        // Not a Bearer token
        return null;
    }

    /// <summary>
    /// Validates a JSON Web Token (JWT) and returns the associated claims principal if the token is valid.
    /// </summary>
    /// <remarks>The token is validated against issuer, audience, signing key, and expiration as specified in
    /// the application configuration. The method does not perform any caching and will throw if required JWT settings
    /// are missing.</remarks>
    /// <param name="token">The JWT string to validate. Must be a non-empty, properly formatted token.</param>
    /// <returns>A <see cref="ClaimsPrincipal"/> representing the authenticated user if the token is valid; otherwise, <see
    /// langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the JWT secret key is not configured in the application settings.</exception>
    private ClaimsPrincipal? ValidateToken(string token)
    {
        // Retrieve JWT settings from configuration
        var jwtSettings = configuration.GetSection("JwtSettings");
        // Get the secret key, issuer, and audience
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        // Set up token validation parameters
        var tokenHandler = new JwtSecurityTokenHandler();
        // Create key from secret
        var key = Encoding.UTF8.GetBytes(secretKey);
        // Configure validation parameters
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Ensure the token is signed with the expected key
            IssuerSigningKey = new SymmetricSecurityKey(key), // Use symmetric key for validation
            ValidateIssuer = !string.IsNullOrEmpty(issuer), // Validate issuer if provided
            ValidIssuer = issuer, // Expected issuer
            ValidateAudience = !string.IsNullOrEmpty(audience), // Validate audience if provided
            ValidAudience = audience, // Expected audience
            ValidateLifetime = true, // Check token expiration
            ClockSkew = TimeSpan.Zero, // No clock skew allowed
        };

        // Validate the token and return the claims principal
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        return principal;
    }
}
