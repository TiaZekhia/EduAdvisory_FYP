using System.Security.Claims;
using EduAdvisory_Backend.DTOs.Messages;
using EduAdvisory_Backend.Services.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetMyConversations()
    {
        var keycloakId = GetKeycloakId();

        var conversations = await _chatService.GetMyConversationsAsync(keycloakId);

        return Ok(conversations);
    }

    [HttpPost("conversations/start")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationDto dto)
    {
        var keycloakId = GetKeycloakId();

        var conversation = await _chatService.StartConversationAsync(keycloakId, dto.StudentId);

        return Ok(conversation);
    }

    [HttpGet("conversations/{conversationId:int}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId)
    {
        var keycloakId = GetKeycloakId();

        var messages = await _chatService.GetMessagesAsync(keycloakId, conversationId);

        return Ok(messages);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var keycloakId = GetKeycloakId();

        var message = await _chatService.SendMessageAsync(keycloakId, dto);

        return Ok(message);
    }

    [HttpPut("conversations/{conversationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int conversationId)
    {
        var keycloakId = GetKeycloakId();

        await _chatService.MarkAsReadAsync(keycloakId, conversationId);

        return NoContent();
    }

    [HttpPost("conversations/my-advisor")]
    public async Task<IActionResult> StartConversationWithMyAdvisor()
    {
        var keycloakId = GetKeycloakId();

        var conversation = await _chatService.StartConversationWithMyAdvisorAsync(keycloakId);

        return Ok(conversation);
    }

    [HttpGet("advisor/students")]
    public async Task<IActionResult> GetMyAssignedStudents()
    {
        var keycloakId = GetKeycloakId();

        var students = await _chatService.GetMyAssignedStudentsAsync(keycloakId);

        return Ok(students);
    }

    [HttpPost("messages/with-files")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SendMessageWithFiles([FromForm] SendMessageWithFileDto dto)
    {
        var keycloakId = GetKeycloakId();

        var message = await _chatService.SendMessageWithFilesAsync(keycloakId, dto);

        return Ok(message);
    }

    [HttpPut("messages/{messageId:int}")]
    public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageDto dto)
    {
        var keycloakId = GetKeycloakId();

        var message = await _chatService.EditMessageAsync(keycloakId, messageId, dto);

        return Ok(message);
    }

    [HttpDelete("messages/{messageId:int}")]
    public async Task<IActionResult> DeleteMessage(int messageId)
    {
        var keycloakId = GetKeycloakId();

        await _chatService.DeleteMessageAsync(keycloakId, messageId);

        return NoContent();
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadMessagesCount()
    {
        var keycloakId = GetKeycloakId();

        var count = await _chatService.GetUnreadMessagesCountAsync(keycloakId);

        return Ok(new { unreadMessagesCount = count });
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