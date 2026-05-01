namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class CreateAdvisorAvailabilityDto
    {
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
    }
}
