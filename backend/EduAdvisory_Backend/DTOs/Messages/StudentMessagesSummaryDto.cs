namespace EduAdvisory_Backend.DTOs.Messages
{
    public class StudentMessagesSummaryDto
    {
        public int TotalMessages { get; set; }
        public int Recent7Days { get; set; }
        public int ThisMonth { get; set; }
    }
}