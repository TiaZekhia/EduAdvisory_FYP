namespace EduAdvisory_Backend.DTOs.Meetings;

public class BookMeetingDto
{
    public DateTimeOffset StartAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? Reason { get; set; }
}