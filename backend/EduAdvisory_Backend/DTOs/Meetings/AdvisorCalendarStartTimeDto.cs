namespace EduAdvisory_Backend.DTOs.Meetings;

public class AdvisorCalendarStartTimeDto
{
    public DateTimeOffset StartAt { get; set; }
    public List<int> AllowedDurations { get; set; } = new();
}