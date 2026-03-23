namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorMeetingDto
    {
        public int MeetingId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public DateTime MeetingDate { get; set; }
        public string MeetingType { get; set; } = "";
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
    }
}