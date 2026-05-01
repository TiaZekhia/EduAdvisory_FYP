namespace EduAdvisory_Backend.DTOs.Meetings;

public class CreateMeetingRequestDto
{
    public DateTimeOffset StartAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? Reason { get; set; }
}