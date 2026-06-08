namespace EduAdvisory_Backend.DTOs.Meetings;

public class AddExceptionDto
{
    public DateOnly Date { get; set; }

    /// <summary>Null = block full day. Provide both to block only a time range.</summary>
    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }
}
