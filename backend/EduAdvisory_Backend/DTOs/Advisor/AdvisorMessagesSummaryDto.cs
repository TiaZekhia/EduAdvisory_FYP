namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorMessagesSummaryDto
    {
        public int TotalMessages { get; set; }
        public int Recent7Days { get; set; }
        public int ThisMonth { get; set; }
    }
}