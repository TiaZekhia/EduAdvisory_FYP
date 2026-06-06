using System.Security.Claims;
using System.Text;
using System.Text.Json;
using EduAdvisory_Backend.DTOs.AI.StudentChat;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Controllers;

[ApiController]
[Route("api/student-ai")]
[Authorize(Roles = "STUDENT")]
public class StudentAiController : ControllerBase
{
    private readonly IStudentAiChatService _studentAiChatService;
    private readonly EduAdvisoryDbContext _dbContext;

    public StudentAiController(
        IStudentAiChatService studentAiChatService,
        EduAdvisoryDbContext dbContext)
    {
        _studentAiChatService = studentAiChatService;
        _dbContext = dbContext;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] StudentAiChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var studentId = await GetCurrentStudentIdAsync(cancellationToken);

            if (studentId == null)
            {
                return Unauthorized(new
                {
                    message = "Student profile is not linked to this account."
                });
            }

            var response = await _studentAiChatService.ChatAsync(
                studentId.Value,
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("chat/stream")]
    public async Task StreamChat(
        [FromBody] StudentAiChatRequest request,
        CancellationToken cancellationToken)
    {
        var studentId = await GetCurrentStudentIdAsync(cancellationToken);

        if (studentId == null)
        {
            Response.StatusCode = 401;
            Response.ContentType = "application/json";
            await Response.WriteAsync(
                "{\"message\":\"Student profile is not linked to this account.\"}",
                cancellationToken);
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            await _studentAiChatService.StreamChatAsync(
                studentId.Value, request, Response.Body, cancellationToken);
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new { message = ex.Message });
            var errorBytes = Encoding.UTF8.GetBytes(
                $"event: error\ndata: {errorJson}\n\nevent: done\ndata: {{}}\n\n");
            await Response.Body.WriteAsync(errorBytes, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    private async Task<int?> GetCurrentStudentIdAsync(
        CancellationToken cancellationToken)
    {
        var username =
            User.Identity?.Name ??
            User.FindFirst("preferred_username")?.Value;

        var keycloakId =
            User.FindFirst("sub")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(username) &&
            string.IsNullOrWhiteSpace(keycloakId))
        {
            return null;
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Username == username ||
                (!string.IsNullOrWhiteSpace(keycloakId) &&
                 u.KeycloakId == keycloakId),
                cancellationToken);

        return user?.LinkedStudentId;
    }
}