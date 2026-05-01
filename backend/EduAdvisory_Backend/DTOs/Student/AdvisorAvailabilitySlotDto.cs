namespace EduAdvisory_Backend.DTOs.Student
{
    public class AdvisorAvailabilitySlotDto
    {
        public int AvailabilityId { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
    }
}
