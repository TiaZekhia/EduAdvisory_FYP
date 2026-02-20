namespace EduAdvisory_Backend.DTOs.Student
{
    public class DegreeProgressDto
    {
        public int CreditsEarned { get; set; }
        public int CreditsRequired { get; set; }
        public decimal PercentComplete { get; set; }
    }
}