using System.Security.Claims;
using EduAdvisory_Backend.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var keycloakId = GetKeycloakId();

        var profile = await _profileService.GetMyProfileAsync(keycloakId);

        return Ok(profile);
    }

    private string GetKeycloakId()
    {
        var keycloakId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(keycloakId))
            throw new UnauthorizedAccessException("Keycloak user id was not found in token.");

        return keycloakId;
    }
}