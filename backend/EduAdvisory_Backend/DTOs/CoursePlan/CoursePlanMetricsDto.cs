namespace EduAdvisory_Backend.DTOs.CoursePlan
{
    public class CoursePlanMetricsDto
    {
        public int SemestersRemaining { get; set; }
        public int CoursesRemaining { get; set; }
        public int CreditsRemaining { get; set; }
        public string EstimatedGraduationTerm { get; set; } = ""; // e.g. "Fall 2027" or "Semester 12 (Fall)"
    }
}
