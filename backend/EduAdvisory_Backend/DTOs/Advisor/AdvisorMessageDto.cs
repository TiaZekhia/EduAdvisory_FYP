namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorMessageDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int RecipientsCount { get; set; }
    }
}