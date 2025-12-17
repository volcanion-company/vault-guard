using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace VaultGuard.API.Conventions;

/// <summary>
/// Convention to convert controller names to kebab-case in routes.
/// Example: UserManagementController -> user-management
///          UserProfileController -> user-profile
/// </summary>
public class KebabCaseRouteConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            // Get the controller name without the "Controller" suffix
            var controllerName = controller.ControllerName;

            // Convert to kebab-case
            var kebabCaseName = ConvertToKebabCase(controllerName);

            // Update all selectors with the new route
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    // Replace [controller] token with kebab-case name
                    selector.AttributeRouteModel.Template = selector.AttributeRouteModel.Template?.Replace("[controller]", kebabCaseName);
                }
            }
        }
    }

    private static string ConvertToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Insert a hyphen before each uppercase letter (except the first one)
        // and convert the entire string to lowercase
        return Regex.Replace(value, "(?<!^)([A-Z])", "-$1", RegexOptions.None, TimeSpan.FromMilliseconds(100)).ToLowerInvariant();
    }
}
