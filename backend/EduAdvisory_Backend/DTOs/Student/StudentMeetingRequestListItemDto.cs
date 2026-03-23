namespace EduAdvisory_Backend.DTOs.Student
{
    public class StudentMeetingRequestListItemDto
    {
        public int RequestId { get; set; }
        public string AdvisorName { get; set; } = "";
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string Status { get; set; } = "";
        public string? Reason { get; set; }
        public string? RejectionReason { get; set; }
    }
}
