using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EduAdvisory_Backend.DTOs.Admin;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services;

public class AdminUserManagementService : IAdminUserManagementService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly EduAdvisoryDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminUserManagementService> _logger;

    public AdminUserManagementService(
        EduAdvisoryDbContext context,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AdminUserManagementService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<AdminManagedUserDto>> GetUsersAsync(CancellationToken ct = default)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Include(u => u.LinkedStudent)
            .Include(u => u.LinkedAdvisor)
            .OrderByDescending(u => u.IsActive)
            .ThenBy(u => u.Role)
            .ThenBy(u => u.Username)
            .ToListAsync(ct);

        return users.Select(MapManagedUser).ToList();
    }

    public async Task<List<AdminAvailableUserOptionDto>> GetAvailableLinksAsync(string role, CancellationToken ct = default)
    {
        var normalizedRole = NormalizeRole(role);

        if (normalizedRole == "student")
        {
            return await _context.SisStudents
                .AsNoTracking()
                .Where(student => !_context.Users.Any(user => user.LinkedStudentId == student.StudentId))
                .OrderBy(student => student.StudentId)
                .Select(student => new AdminAvailableUserOptionDto
                {
                    LinkedEntityId = student.StudentId,
                    Role = normalizedRole,
                    DisplayName = ((student.FirstName ?? string.Empty) + " " + (student.LastName ?? string.Empty)).Trim(),
                    Email = student.Email,
                    SecondaryText = $"ID {student.StudentId} • {student.ProgramCode ?? "No program"}"
                })
                .ToListAsync(ct);
        }

        return await _context.Advisors
            .AsNoTracking()
            .Where(advisor => !_context.Users.Any(user => user.LinkedAdvisorId == advisor.AdvisorId))
            .OrderBy(advisor => advisor.AdvisorId)
            .Select(advisor => new AdminAvailableUserOptionDto
            {
                LinkedEntityId = advisor.AdvisorId,
                Role = normalizedRole,
                DisplayName = advisor.Name,
                Email = advisor.Email,
                SecondaryText = $"ID {advisor.AdvisorId}"
            })
            .ToListAsync(ct);
    }

    public async Task<AdminManagedUserDto> CreateUserAsync(CreateManagedUserDto dto, CancellationToken ct = default)
    {
        var normalizedRole = NormalizeRole(dto.Role);
        var databaseRole = ToDatabaseRole(normalizedRole);
        var username = NormalizeUsername(dto.Username);

        await EnsureLocalUsernameAvailableAsync(username, null, ct);
        await EnsureKeycloakUsernameAvailableAsync(username, null, ct);
        await EnsureLinkIsAvailableAsync(normalizedRole, dto.LinkedEntityId, ct);

        var linkedProfile = await GetLinkedProfileAsync(normalizedRole, dto.LinkedEntityId, ct);
        var keycloakUserId = await CreateKeycloakUserAsync(username, dto.Password, normalizedRole, linkedProfile, ct);

        try
        {
            var user = new User
            {
                Username = username,
                KeycloakId = keycloakUserId,
                Role = databaseRole,
                IsActive = true,
                LinkedStudentId = normalizedRole == "student" ? dto.LinkedEntityId : null,
                LinkedAdvisorId = normalizedRole == "advisor" ? dto.LinkedEntityId : null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            await LoadLinksAsync(user, ct);
            return MapManagedUser(user);
        }
        catch
        {
            await TryDeleteKeycloakUserAsync(keycloakUserId, ct);
            throw;
        }
    }

    public async Task<AdminManagedUserDto> UpdateUserAsync(int userId, UpdateManagedUserDto dto, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.LinkedStudent)
            .Include(u => u.LinkedAdvisor)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        if (string.IsNullOrWhiteSpace(user.KeycloakId))
            throw new InvalidOperationException("This user is not linked to Keycloak.");

        await UpdateKeycloakUserAsync(
            user.KeycloakId,
            user.Username ?? throw new InvalidOperationException("This user does not have a username."),
            user.IsActive,
            dto.Password,
            ct);
        await _context.SaveChangesAsync(ct);

        return MapManagedUser(user);
    }

    public async Task<AdminManagedUserDto> DeactivateUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.LinkedStudent)
            .Include(u => u.LinkedAdvisor)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        if (!user.IsActive)
            return MapManagedUser(user);

        if (string.IsNullOrWhiteSpace(user.KeycloakId))
            throw new InvalidOperationException("This user is not linked to Keycloak.");

        await SetKeycloakUserEnabledAsync(user.KeycloakId, false, ct);

        user.IsActive = false;
        await _context.SaveChangesAsync(ct);

        return MapManagedUser(user);
    }

    public async Task<AdminManagedUserDto> ReactivateUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.LinkedStudent)
            .Include(u => u.LinkedAdvisor)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        if (user.IsActive)
            return MapManagedUser(user);

        if (string.IsNullOrWhiteSpace(user.KeycloakId))
            throw new InvalidOperationException("This user is not linked to Keycloak.");

        await SetKeycloakUserEnabledAsync(user.KeycloakId, true, ct);

        user.IsActive = true;
        await _context.SaveChangesAsync(ct);

        return MapManagedUser(user);
    }

    private async Task EnsureLocalUsernameAvailableAsync(string username, int? currentUserId, CancellationToken ct)
    {
        var exists = await _context.Users.AnyAsync(
            user => user.Username == username && (!currentUserId.HasValue || user.UserId != currentUserId.Value),
            ct);

        if (exists)
            throw new InvalidOperationException("Username is already used in the local users table.");
    }

    private async Task EnsureKeycloakUsernameAvailableAsync(string username, string? currentKeycloakUserId, CancellationToken ct)
    {
        var accessToken = await GetAdminAccessTokenAsync(ct);
        var existingKeycloakUserId = await FindKeycloakUserIdByUsernameAsync(accessToken, username, ct);

        if (string.IsNullOrWhiteSpace(existingKeycloakUserId))
            return;

        if (!string.IsNullOrWhiteSpace(currentKeycloakUserId) &&
            string.Equals(existingKeycloakUserId, currentKeycloakUserId, StringComparison.Ordinal))
            return;

        throw new InvalidOperationException("A Keycloak user with this username already exists. Choose another username.");
    }

    private async Task EnsureLinkIsAvailableAsync(string role, int linkedEntityId, CancellationToken ct)
    {
        var inUse = role == "student"
            ? await _context.Users.AnyAsync(user => user.LinkedStudentId == linkedEntityId, ct)
            : await _context.Users.AnyAsync(user => user.LinkedAdvisorId == linkedEntityId, ct);

        if (inUse)
            throw new InvalidOperationException("The selected record is already registered as a user.");
    }

    private async Task<LinkedProfile> GetLinkedProfileAsync(string role, int linkedEntityId, CancellationToken ct)
    {
        if (role == "student")
        {
            var student = await _context.SisStudents
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.StudentId == linkedEntityId, ct)
                ?? throw new InvalidOperationException("Student not found.");

            return new LinkedProfile(
                ((student.FirstName ?? string.Empty) + " " + (student.LastName ?? string.Empty)).Trim(),
                student.Email,
                student.FirstName,
                student.LastName);
        }

        var advisor = await _context.Advisors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.AdvisorId == linkedEntityId, ct)
            ?? throw new InvalidOperationException("Advisor not found.");

        var nameParts = advisor.Name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        return new LinkedProfile(
            advisor.Name,
            advisor.Email,
            nameParts.ElementAtOrDefault(0),
            nameParts.ElementAtOrDefault(1));
    }

    private async Task LoadLinksAsync(User user, CancellationToken ct)
    {
        if (user.LinkedStudentId.HasValue)
        {
            user.LinkedStudent = await _context.SisStudents
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.StudentId == user.LinkedStudentId.Value, ct);
        }

        if (user.LinkedAdvisorId.HasValue)
        {
            user.LinkedAdvisor = await _context.Advisors
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.AdvisorId == user.LinkedAdvisorId.Value, ct);
        }
    }

    private AdminManagedUserDto MapManagedUser(User user)
    {
        var isStudent = string.Equals(user.Role, "student", StringComparison.OrdinalIgnoreCase);
        var linkedDisplayName = isStudent
            ? ((user.LinkedStudent?.FirstName ?? string.Empty) + " " + (user.LinkedStudent?.LastName ?? string.Empty)).Trim()
            : user.LinkedAdvisor?.Name ?? string.Empty;
        var linkedEmail = isStudent ? user.LinkedStudent?.Email : user.LinkedAdvisor?.Email;
        var secondaryText = isStudent
            ? $"ID {user.LinkedStudentId} • {user.LinkedStudent?.ProgramCode ?? "No program"}"
            : $"ID {user.LinkedAdvisorId}";

        return new AdminManagedUserDto
        {
            UserId = user.UserId,
            Username = user.Username ?? string.Empty,
            Role = (user.Role ?? string.Empty).ToLowerInvariant(),
            IsActive = user.IsActive,
            KeycloakId = user.KeycloakId,
            LinkedEntityId = isStudent ? user.LinkedStudentId ?? 0 : user.LinkedAdvisorId ?? 0,
            LinkedDisplayName = linkedDisplayName,
            LinkedEmail = linkedEmail,
            SecondaryText = secondaryText
        };
    }

    private static string NormalizeRole(string role)
    {
        var normalizedRole = role.Trim().ToLowerInvariant();
        if (normalizedRole is not ("student" or "advisor"))
            throw new InvalidOperationException("Role must be either student or advisor.");

        return normalizedRole;
    }

    private static string ToDatabaseRole(string normalizedRole)
    {
        return normalizedRole.ToUpperInvariant();
    }

    private static string NormalizeUsername(string username)
    {
        var normalized = username.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Username is required.");

        return normalized;
    }

    private async Task<string> CreateKeycloakUserAsync(
        string username,
        string password,
        string role,
        LinkedProfile linkedProfile,
        CancellationToken ct)
    {
        var accessToken = await GetAdminAccessTokenAsync(ct);
        var endpoint = GetAdminUsersEndpoint();
        var roleName = role.ToUpperInvariant();

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        createRequest.Content = JsonContent(new
        {
            username,
            enabled = true,
            email = linkedProfile.Email,
            firstName = linkedProfile.FirstName,
            lastName = linkedProfile.LastName,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
        });

        using var createResponse = await _httpClient.SendAsync(createRequest, ct);
        var createBody = await createResponse.Content.ReadAsStringAsync(ct);

        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            _logger.LogError("Keycloak create user failed. Status={StatusCode}, Body={Body}", createResponse.StatusCode, createBody);
            throw new InvalidOperationException(ExtractKeycloakError(createBody, "Failed to create Keycloak user."));
        }

        var keycloakUserId = createResponse.Headers.Location?.Segments.LastOrDefault()?.Trim('/');
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            keycloakUserId = await FindKeycloakUserIdByUsernameAsync(accessToken, username, ct);
        }

        if (string.IsNullOrWhiteSpace(keycloakUserId))
            throw new InvalidOperationException("Keycloak user was created but its identifier could not be resolved.");

        await AssignRealmRoleAsync(accessToken, keycloakUserId, roleName, ct);
        return keycloakUserId;
    }

    private async Task UpdateKeycloakUserAsync(
        string keycloakUserId,
        string username,
        bool isEnabled,
        string? password,
        CancellationToken ct)
    {
        var accessToken = await GetAdminAccessTokenAsync(ct);

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"{GetAdminUsersEndpoint()}/{keycloakUserId}");
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        updateRequest.Content = JsonContent(new
        {
            username,
            enabled = isEnabled
        });

        using var updateResponse = await _httpClient.SendAsync(updateRequest, ct);
        var updateBody = await updateResponse.Content.ReadAsStringAsync(ct);

        if (!updateResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Keycloak update user failed. Status={StatusCode}, Body={Body}", updateResponse.StatusCode, updateBody);
            throw new InvalidOperationException(ExtractKeycloakError(updateBody, "Failed to update Keycloak user."));
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            using var passwordRequest = new HttpRequestMessage(HttpMethod.Put, $"{GetAdminUsersEndpoint()}/{keycloakUserId}/reset-password");
            passwordRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            passwordRequest.Content = JsonContent(new
            {
                type = "password",
                value = password,
                temporary = false
            });

            using var passwordResponse = await _httpClient.SendAsync(passwordRequest, ct);
            var passwordBody = await passwordResponse.Content.ReadAsStringAsync(ct);

            if (!passwordResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Keycloak reset password failed. Status={StatusCode}, Body={Body}", passwordResponse.StatusCode, passwordBody);
                throw new InvalidOperationException(ExtractKeycloakError(passwordBody, "Failed to reset Keycloak password."));
            }
        }
    }

    private async Task SetKeycloakUserEnabledAsync(string keycloakUserId, bool isEnabled, CancellationToken ct)
    {
        var accessToken = await GetAdminAccessTokenAsync(ct);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"{GetAdminUsersEndpoint()}/{keycloakUserId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent(new { enabled = isEnabled });

        using var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Keycloak enable/disable user failed. Status={StatusCode}, Body={Body}", response.StatusCode, body);
            throw new InvalidOperationException(ExtractKeycloakError(body, "Failed to update Keycloak user status."));
        }
    }

    private async Task TryDeleteKeycloakUserAsync(string keycloakUserId, CancellationToken ct)
    {
        try
        {
            var accessToken = await GetAdminAccessTokenAsync(ct);
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{GetAdminUsersEndpoint()}/{keycloakUserId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            await _httpClient.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to roll back Keycloak user {KeycloakUserId}", keycloakUserId);
        }
    }

    private async Task<string> GetAdminAccessTokenAsync(CancellationToken ct)
    {
        var authority = _configuration["Keycloak:Authority"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Keycloak authority is not configured.");
        var clientId = _configuration["Keycloak:AdminClientId"];
        var clientSecret = _configuration["Keycloak:AdminClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Keycloak admin client credentials are not configured.");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{authority}/protocol/openid-connect/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        });

        using var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Keycloak admin token request failed. Status={StatusCode}, Body={Body}", response.StatusCode, body);
            throw new InvalidOperationException("Failed to authenticate with Keycloak Admin API.");
        }

        using var document = JsonDocument.Parse(body);
        var accessToken = document.RootElement.GetProperty("access_token").GetString();

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("Keycloak admin token response did not contain an access token.");

        return accessToken;
    }

    private async Task AssignRealmRoleAsync(string accessToken, string keycloakUserId, string roleName, CancellationToken ct)
    {
        var roleEndpoint = $"{GetAdminRealmRolesEndpoint()}/{roleName}";

        using var roleRequest = new HttpRequestMessage(HttpMethod.Get, roleEndpoint);
        roleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var roleResponse = await _httpClient.SendAsync(roleRequest, ct);
        var roleBody = await roleResponse.Content.ReadAsStringAsync(ct);

        if (!roleResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Keycloak role lookup failed. Status={StatusCode}, Body={Body}", roleResponse.StatusCode, roleBody);
            throw new InvalidOperationException($"Keycloak role '{roleName}' was not found.");
        }

        using var assignRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{GetAdminUsersEndpoint()}/{keycloakUserId}/role-mappings/realm");
        assignRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        assignRequest.Content = new StringContent($"[{roleBody}]", Encoding.UTF8, "application/json");

        using var assignResponse = await _httpClient.SendAsync(assignRequest, ct);
        var assignBody = await assignResponse.Content.ReadAsStringAsync(ct);

        if (!assignResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Keycloak role assignment failed. Status={StatusCode}, Body={Body}", assignResponse.StatusCode, assignBody);
            throw new InvalidOperationException($"Failed to assign Keycloak role '{roleName}'.");
        }
    }

    private async Task<string?> FindKeycloakUserIdByUsernameAsync(string accessToken, string username, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{GetAdminUsersEndpoint()}?exact=true&username={Uri.EscapeDataString(username)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return null;

        using var document = JsonDocument.Parse(body);
        return document.RootElement.EnumerateArray()
            .Select(element => element.TryGetProperty("id", out var idProperty) ? idProperty.GetString() : null)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
    }

    private string GetAdminUsersEndpoint()
    {
        var adminBaseUrl = _configuration["Keycloak:AdminApiBaseUrl"];

        if (string.IsNullOrWhiteSpace(adminBaseUrl))
        {
            var authority = _configuration["Keycloak:Authority"]
                ?? throw new InvalidOperationException("Keycloak authority is not configured.");
            adminBaseUrl = new Uri(authority).GetLeftPart(UriPartial.Authority);
        }

        return $"{adminBaseUrl.TrimEnd('/')}/admin/realms/{GetRealmName()}/users";
    }

    private string GetAdminRealmRolesEndpoint()
    {
        var usersEndpoint = GetAdminUsersEndpoint();
        return usersEndpoint[..usersEndpoint.LastIndexOf("/users", StringComparison.Ordinal)] + "/roles";
    }

    private string GetRealmName()
    {
        var configuredRealm = _configuration["Keycloak:Realm"];
        if (!string.IsNullOrWhiteSpace(configuredRealm))
            return configuredRealm;

        var authority = _configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Keycloak authority is not configured.");
        var authorityUri = new Uri(authority);
        var segments = authorityUri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var realmIndex = Array.FindIndex(segments, segment => string.Equals(segment, "realms", StringComparison.OrdinalIgnoreCase));

        if (realmIndex < 0 || realmIndex == segments.Length - 1)
            throw new InvalidOperationException("Unable to determine Keycloak realm from authority.");

        return segments[realmIndex + 1];
    }

    private static StringContent JsonContent(object payload)
    {
        return new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");
    }

    private static string ExtractKeycloakError(string responseBody, string fallbackMessage)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return fallbackMessage;

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (document.RootElement.TryGetProperty("errorMessage", out var errorMessage))
                    return errorMessage.GetString() ?? fallbackMessage;

                if (document.RootElement.TryGetProperty("error_description", out var errorDescription))
                    return errorDescription.GetString() ?? fallbackMessage;

                if (document.RootElement.TryGetProperty("error", out var error))
                    return error.GetString() ?? fallbackMessage;
            }
        }
        catch
        {
        }

        return responseBody;
    }

    private sealed record LinkedProfile(string DisplayName, string? Email, string? FirstName, string? LastName);
}