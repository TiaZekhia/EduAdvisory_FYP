namespace EduAdvisory_Backend.DTOs.Broadcasts;

public class BroadcastDto
{
    public int BroadcastMessageId { get; set; }

    public int AdvisorId { get; set; }

    public string AdvisorName { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}