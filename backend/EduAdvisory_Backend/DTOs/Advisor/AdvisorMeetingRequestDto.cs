namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorMeetingRequestDto
    {
        public int RequestId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "";
        public DateTimeOffset RequestedAt { get; set; }
    }
}
