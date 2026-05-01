namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorRecentActivityDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string TimeAgo { get; set; } = "";
        public DateTimeOffset CreatedAt { get; set; }
    }
}