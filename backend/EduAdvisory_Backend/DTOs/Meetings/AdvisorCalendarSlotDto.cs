namespace EduAdvisory_Backend.DTOs.Meetings;

public class AdvisorCalendarSlotDto
{
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public bool IsAvailable { get; set; }
}
