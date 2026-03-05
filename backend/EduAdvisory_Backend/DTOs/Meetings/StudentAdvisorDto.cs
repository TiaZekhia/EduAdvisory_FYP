namespace EduAdvisory_Backend.DTOs.Meetings
{
    public class StudentAdvisorDto
    {
        public int AdvisorId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";

        // UI shows office hours; not in schema => hardcode for now
        public string OfficeHours { get; set; } = "Mon, Wed, Fri: 2-4 PM";
        public string Availability { get; set; } = "Available";
    }
}
