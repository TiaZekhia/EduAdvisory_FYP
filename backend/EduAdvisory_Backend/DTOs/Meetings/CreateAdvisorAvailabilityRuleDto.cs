namespace EduAdvisory_Backend.DTOs.Meetings;

public class CreateAdvisorAvailabilityRuleDto
{
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}