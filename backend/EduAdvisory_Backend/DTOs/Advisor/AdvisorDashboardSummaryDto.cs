namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorDashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int OnProbation { get; set; }
        public decimal AverageGpa { get; set; }
        public int UpcomingMeetings { get; set; }
    }
}