using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EduAdvisory_Backend.Extensions
{
    public static class JwtBearerEventsConfiguration
    {
        public static JwtBearerEvents Configure()
        {
            return new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Skip authentication for OPTIONS requests (CORS preflight)
                    if (context.Request.Method == "OPTIONS")
                    {
                        context.NoResult();
                        return Task.CompletedTask;
                    }

                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogError(
                        "Authentication failed: {Message}. Exception: {ExceptionType}",
                        context.Exception.Message,
                        context.Exception.GetType().Name
                    );

                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogWarning(
                        "Authentication challenge triggered. Error: {Error}, Description: {ErrorDescription}",
                        context.Error,
                        context.ErrorDescription
                    );

                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Token validated successfully");

                    var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                    if (claimsIdentity == null)
                    {
                        logger.LogWarning("ClaimsIdentity is null after token validation");
                        return Task.CompletedTask;
                    }

                    // Extract and add roles from realm_access claim
                    ExtractAndAddRoles(context, claimsIdentity, logger);

                    return Task.CompletedTask;
                }
            };
        }

        private static void ExtractAndAddRoles(
            TokenValidatedContext context,
            ClaimsIdentity claimsIdentity,
            ILogger logger)
        {
            // Look for direct role claims
            var directRoles = context.Principal?.Claims
                .Where(c => c.Type == "role" || c.Type == "roles")
                .ToList();

            if (directRoles?.Any() == true)
            {
                foreach (var role in directRoles)
                {
                    claimsIdentity.AddClaim(
                        new Claim(ClaimTypes.Role, role.Value)
                    );
                }
            }

            // Extract from realm_access claim
            var realmAccessClaim = context.Principal?.Claims
                .FirstOrDefault(c => c.Type == "realm_access" || c.Type.EndsWith("/realm_access"));

            if (realmAccessClaim != null)
            {
                try
                {
                    var realmAccessJson = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                    if (realmAccessJson.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrEmpty(roleValue))
                            {
                                claimsIdentity.AddClaim(
                                    new Claim(ClaimTypes.Role, roleValue)
                                );
                                logger.LogDebug("Added role from realm_access: {Role}", roleValue);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing realm_access claim");
                }
            }

            // Log roles for debugging (only in Development)
            var finalRoles = context.Principal?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (finalRoles?.Any() == true)
            {
                logger.LogDebug("User roles: {Roles}", string.Join(", ", finalRoles));
            }
            else
            {
                logger.LogWarning("No roles found for user");
            }
        }
    }
}