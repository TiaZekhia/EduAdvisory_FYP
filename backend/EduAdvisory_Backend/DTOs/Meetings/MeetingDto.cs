namespace EduAdvisory_Backend.DTOs.Meetings
{
    public class MeetingDto
    {
        public int MeetingId { get; set; }
        public string Title { get; set; } = "";
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string Status { get; set; } = "";
        public string? MeetingType { get; set; }
        public string? MeetingLink { get; set; }
        public string? Notes { get; set; }
        public string StudentName { get; set; } = "";
        public string AdvisorName { get; set; } = "";
        public string? AdvisorEmail { get; set; }
    }
}
