namespace EduAdvisory_Backend.DTOs.Student
{
    public class StudentAlertDto
    {
        public string Severity { get; set; } // "HIGH", "MEDIUM", "LOW"
        public string Title { get; set; }
        public string Message { get; set; }

        public string? CourseCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentAlertsCountDto
    {
        public int Count { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
    }
}