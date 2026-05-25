using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace EduAdvisory_Backend.Extensions;

/// <summary>
/// Transforms Keycloak realm roles from nested "realm_access.roles" claim
/// into standard ClaimTypes.Role claims for ASP.NET authorization.
/// 
/// Keycloak JWT structure:
/// {
///   "realm_access": {
///     "roles": ["AutomationAdmin", "user"]
///   }
/// }
/// 
/// This transformation extracts those roles and adds them as individual role claims.
/// </summary>
public class KeycloakClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimsIdentity = principal.Identity as ClaimsIdentity;
        if (claimsIdentity is null)
            return Task.FromResult(principal);

        // Extract realm_access claim
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim == null)
            return Task.FromResult(principal);

        try
        {
            // Parse the JSON structure
            using var doc = JsonDocument.Parse(realmAccessClaim.Value);
            var rolesElement = doc.RootElement.GetProperty("roles");

            // Add each role as a separate claim
            foreach (var role in rolesElement.EnumerateArray())
            {
                var roleValue = role.GetString();
                if (!string.IsNullOrEmpty(roleValue))
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                }
            }
        }
        catch
        {
            // If parsing fails, continue without role claims
        }

        return Task.FromResult(principal);
    }
}
