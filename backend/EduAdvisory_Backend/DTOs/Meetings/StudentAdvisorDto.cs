namespace EduAdvisory_Backend.DTOs.Meetings
{
    public class StudentAdvisorDto
    {
        public int AdvisorId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Office { get; set; } = "";
        public string OfficeHours { get; set; } = "Mon, Wed, Fri: 2-4 PM";
        public string Availability { get; set; } = "Available";
    }
}
