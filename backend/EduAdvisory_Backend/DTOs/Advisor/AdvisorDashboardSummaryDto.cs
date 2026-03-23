namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorDashboardSummaryDto
    {
        public int TotalAdvisees { get; set; }
        public int UpcomingMeetings { get; set; }
        public int TotalMeetings { get; set; }
        public int TotalAnnouncements { get; set; }
    }
}