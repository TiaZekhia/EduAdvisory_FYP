namespace EduAdvisory_Backend.DTOs.Student
{
    public class ProgressSummaryDto
    {
        public int CreditsEarned { get; set; }
        public int CreditsRequired { get; set; }
        public int CreditsRemaining { get; set; }
        public decimal PercentComplete { get; set; }

        public int CoursesPassed { get; set; }
        public int CoursesFailed { get; set; }
    }
}