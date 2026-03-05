namespace EduAdvisory_Backend.DTOs.Meetings
{
    public class StudentMeetingDto
    {
        public int MeetingId { get; set; }
        public string Title { get; set; } = "";          // derived from meeting_type
        public DateTimeOffset MeetingDate { get; set; }
        public string MeetingType { get; set; } = "";
        public string Status { get; set; } = "";         // "scheduled" or "completed" (derived)
        public string? Notes { get; set; }

        // advisor info (joined)
        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = "";
        public string AdvisorEmail { get; set; } = "";
    }
}
