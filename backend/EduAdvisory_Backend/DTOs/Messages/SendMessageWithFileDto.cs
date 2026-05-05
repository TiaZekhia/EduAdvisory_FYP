using Microsoft.AspNetCore.Http;

namespace EduAdvisory_Backend.DTOs.Messages;

public class SendMessageWithFileDto
{
    public int ConversationId { get; set; }

    public string? Content { get; set; }

    public List<IFormFile>? Files { get; set; }
}