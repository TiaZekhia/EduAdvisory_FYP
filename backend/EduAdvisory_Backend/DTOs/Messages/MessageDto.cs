namespace EduAdvisory_Backend.DTOs.Messages;

public class MessageDto
{
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public int SenderUserId { get; set; }

    public string SenderRole { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public bool IsMine { get; set; }
}