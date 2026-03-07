namespace EduAdvisory_Backend.DTOs.Messages
{
    public class StudentMessagesAdvisorDto
    {
        public int AdvisorId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";

        public string Office { get; set; } = "Building A, Room 305";
        public string OfficeHours { get; set; } = "Mon, Wed, Fri: 2-4 PM";
    }
}