using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EduAdvisory_Backend.Models;
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
                    if (context.Request.Method == "OPTIONS")
                    {
                        context.NoResult();
                        return Task.CompletedTask;
                    }

                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs/chat"))
                    {
                        context.Token = accessToken;
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

                OnTokenValidated = async context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogInformation("Token validated successfully");

                    var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                    if (claimsIdentity == null)
                    {
                        logger.LogWarning("ClaimsIdentity is null after token validation");
                        return;
                    }

                    // Extract and add roles from realm_access claim
                    ExtractAndAddRoles(context, claimsIdentity, logger);

                    await ValidateLocalUserAccessAsync(context, claimsIdentity, logger);

                }
            };
        }

        private static async Task ValidateLocalUserAccessAsync(
            TokenValidatedContext context,
            ClaimsIdentity claimsIdentity,
            ILogger logger)
        {
            var roleSet = claimsIdentity.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value.ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!roleSet.Contains("STUDENT") && !roleSet.Contains("ADVISOR"))
            {
                return;
            }

            var username = context.Principal?.Identity?.Name
                ?? context.Principal?.FindFirst("preferred_username")?.Value;
            var keycloakId = context.Principal?.FindFirst("sub")?.Value
                ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(username))
            {
                logger.LogWarning("Validated token is missing preferred username for a managed user.");
                context.Fail("The authenticated account is missing a username.");
                return;
            }

            var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<EduAdvisoryDbContext>();

            var user = await dbContext.Users.FirstOrDefaultAsync(u =>
                u.Username == username ||
                (!string.IsNullOrWhiteSpace(keycloakId) && u.KeycloakId == keycloakId));

            if (user == null)
            {
                logger.LogWarning("Blocked login for unregistered managed user {Username}", username);
                context.Fail("This account has not been activated in EduAdvisory.");
                return;
            }

            if (!user.IsActive)
            {
                logger.LogWarning("Blocked login for deactivated managed user {Username}", username);
                context.Fail("This account is deactivated.");
                return;
            }

            var localRole = user.Role?.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(localRole) || !roleSet.Contains(localRole))
            {
                logger.LogWarning(
                    "Blocked login for {Username} because local role {LocalRole} does not match token roles {TokenRoles}",
                    username,
                    user.Role,
                    string.Join(", ", roleSet));
                context.Fail("This account role is not allowed for the current login.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(keycloakId) && !string.Equals(user.KeycloakId, keycloakId, StringComparison.Ordinal))
            {
                user.KeycloakId = keycloakId;
                await dbContext.SaveChangesAsync();
            }
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