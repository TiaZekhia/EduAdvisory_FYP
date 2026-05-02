namespace EduAdvisory_Backend.DTOs.Messages;

public class ConversationDto
{
    public int ConversationId { get; set; }

    public int AdvisorId { get; set; }

    public string AdvisorName { get; set; } = null!;

    public int StudentId { get; set; }

    public string StudentName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? LastMessage { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public int UnreadCount { get; set; }
}