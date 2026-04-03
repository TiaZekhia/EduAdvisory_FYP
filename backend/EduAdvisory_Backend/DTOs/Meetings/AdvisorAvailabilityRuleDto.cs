namespace EduAdvisory_Backend.DTOs.Meetings;

public class AdvisorAvailabilityRuleDto
{
    public int RuleId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; }
}