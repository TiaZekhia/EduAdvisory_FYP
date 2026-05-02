namespace EduAdvisory_Backend.DTOs.Messages;

public class SendMessageDto
{
    public int ConversationId { get; set; }

    public string Content { get; set; } = null!;
}