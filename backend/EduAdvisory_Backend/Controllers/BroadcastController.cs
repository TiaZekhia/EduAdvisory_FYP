using System.Security.Claims;
using EduAdvisory_Backend.DTOs.Broadcasts;
using EduAdvisory_Backend.Services.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/broadcasts")]
public class BroadcastController : ControllerBase
{
    private readonly IBroadcastService _broadcastService;

    public BroadcastController(IBroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateBroadcast([FromForm] CreateBroadcastDto dto)
    {
        var keycloakId = GetKeycloakId();

        var broadcast = await _broadcastService.CreateBroadcastAsync(keycloakId, dto);

        return Ok(broadcast);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyBroadcasts()
    {
        var keycloakId = GetKeycloakId();

        var broadcasts = await _broadcastService.GetMyBroadcastsAsync(keycloakId);

        return Ok(broadcasts);
    }

    [HttpPut("{broadcastMessageId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int broadcastMessageId)
    {
        var keycloakId = GetKeycloakId();

        await _broadcastService.MarkBroadcastAsReadAsync(keycloakId, broadcastMessageId);

        return NoContent();
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