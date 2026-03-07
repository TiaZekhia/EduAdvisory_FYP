namespace EduAdvisory_Backend.DTOs.Messages
{
    public class StudentMessageDto
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        public int AdvisorId { get; set; }
        public string AdvisorName { get; set; } = "";

        public int RecipientsCount { get; set; }
    }
}